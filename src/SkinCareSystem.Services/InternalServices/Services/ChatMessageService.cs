using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Common.DTOs.Pagination;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class ChatMessageService : IChatMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHostEnvironment _environment;
        private readonly ICloudinaryService? _cloudinaryService;

        public ChatMessageService(
            IUnitOfWork unitOfWork,
            IHostEnvironment environment,
            ICloudinaryService? cloudinaryService = null)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _cloudinaryService = cloudinaryService;
        }

        public async Task<IServiceResult> CreateMessageAsync(ChatMessageCreateDto dto)
        {
            var session = await _unitOfWork.ChatSessionRepository.GetByIdAsync(dto.SessionId);
            if (session == null)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Session not found");
            }

            if (!dto.UserId.HasValue)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "UserId is required for chat messages");
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(dto.UserId.Value);
            if (user == null)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "User not found");
            }

            var imageUrl = dto.ImageUrl;
            if (dto.Image is { Length: > 0 })
            {
                imageUrl = await UploadImageAsync(dto.Image).ConfigureAwait(false);
            }

            var hasText = !string.IsNullOrWhiteSpace(dto.Content);
            var hasImage = !string.IsNullOrWhiteSpace(imageUrl);

            if (!hasText && !hasImage)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Either content or image is required.");
            }

            dto.ImageUrl = imageUrl;

            var messageType = DetermineMessageType(hasText, hasImage);
            var timestamp = DateTimeHelper.UtcNowUnspecified();

            var entity = dto.ToEntity(messageType, timestamp);

            await _unitOfWork.ChatMessageRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(StatusCodes.Status201Created, Const.SUCCESS_CREATE_MSG, entity.ToDto());
        }

        public async Task<IServiceResult> UploadImageMessageAsync(Guid sessionId, Guid userId, IFormFile file, string? messageType = null)
        {
            if (file == null || file.Length == 0)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Image file is required");
            }

            var session = await _unitOfWork.ChatSessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Session not found");
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "User not found");
            }

            var typeValue = string.IsNullOrWhiteSpace(messageType) ? "image" : messageType.ToLowerInvariant();

            string imageUrl;
            if (_cloudinaryService != null)
            {
                try
                {
                    var upload = await _cloudinaryService.UploadFileAsync(file, "chat-messages").ConfigureAwait(false);
                    imageUrl = string.IsNullOrWhiteSpace(upload.SecureUrl) ? upload.Url : upload.SecureUrl;
                }
                catch
                {
                    imageUrl = await SaveImageLocallyAsync(file).ConfigureAwait(false);
                }
            }
            else
            {
                imageUrl = await SaveImageLocallyAsync(file).ConfigureAwait(false);
            }

            var entity = new ChatMessageCreateDto
            {
                SessionId = sessionId,
                UserId = userId,
                ImageUrl = imageUrl
            }.ToEntity(typeValue, DateTimeHelper.UtcNowUnspecified());

            await _unitOfWork.ChatMessageRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(StatusCodes.Status201Created, Const.SUCCESS_CREATE_MSG, entity.ToDto());
        }

        private async Task<string> SaveImageLocallyAsync(IFormFile file)
        {
            var uploadsRoot = Path.Combine(_environment.ContentRootPath, "uploads", "chat-images");
            Directory.CreateDirectory(uploadsRoot);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var physicalPath = Path.Combine(uploadsRoot, fileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/chat-images/{fileName}";
        }

        private static string DetermineMessageType(bool hasText, bool hasImage)
        {
            return (hasText, hasImage) switch
            {
                (true, true) => "mixed",
                (false, true) => "image",
                _ => "text"
            };
        }

        private async Task<string> UploadImageAsync(IFormFile file)
        {
            if (_cloudinaryService != null)
            {
                try
                {
                    var upload = await _cloudinaryService.UploadFileAsync(file, "chat-messages").ConfigureAwait(false);
                    return string.IsNullOrWhiteSpace(upload.SecureUrl) ? upload.Url : upload.SecureUrl;
                }
                catch
                {
                    // fall back to local storage
                }
            }

            return await SaveImageLocallyAsync(file).ConfigureAwait(false);
        }

        public async Task<IServiceResult> GetMessageAsync(Guid messageId)
        {
            var message = await _unitOfWork.ChatMessageRepository.GetByIdAsync(messageId);
            if (message == null)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Message not found");
            }

            return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, message.ToDto());
        }

        public async Task<IServiceResult> GetMessagesBySessionAsync(Guid sessionId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, Const.ERROR_INVALID_DATA_MSG);
            }

            var count = await _unitOfWork.ChatMessageRepository.CountBySessionAsync(sessionId);
            if (count == 0)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "No messages found for session");
            }

            var messages = await _unitOfWork.ChatMessageRepository.GetBySessionAsync(sessionId, pageNumber, pageSize);
            var items = messages.Select(m => m.ToDto()).ToList();

            var paged = new PagedResult<ChatMessageDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = count,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };

            return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, paged);
        }
    }
}
