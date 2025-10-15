using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Common.DTOs.Pagination;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class ChatSessionService : IChatSessionService
    {
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "active",
            "archived"
        };

        private readonly IUnitOfWork _unitOfWork;

        public ChatSessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> CreateSessionAsync(ChatSessionCreateDto dto)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "User not found");
            }

            var entity = dto.ToEntity();
            await _unitOfWork.ChatSessionRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(StatusCodes.Status201Created, Const.SUCCESS_CREATE_MSG, entity.ToDto());
        }

        public async Task<IServiceResult> GetSessionAsync(Guid sessionId, bool includeMessages = false)
        {
            var session = await _unitOfWork.ChatSessionRepository.GetByIdAsync(sessionId, includeMessages);
            if (session == null)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Session not found");
            }

            if (!includeMessages)
            {
                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, session.ToDto());
            }

            var dto = new
            {
                session.SessionId,
                session.UserId,
                session.Title,
                Status = session.Status ?? "active",
                session.CreatedAt,
                session.UpdatedAt,
                messages = session.ChatMessages.Select(m => m.ToDto()).ToList()
            };

            return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, dto);
        }

        public async Task<IServiceResult> GetSessionsAsync(Guid? userId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, Const.ERROR_INVALID_DATA_MSG);
            }

            if (userId.HasValue)
            {
                var total = await _unitOfWork.ChatSessionRepository.CountByUserAsync(userId.Value);
                if (total == 0)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                var sessions = await _unitOfWork.ChatSessionRepository.GetByUserAsync(userId.Value, pageNumber, pageSize);
                var dtos = sessions.Select(s => s.ToDto()).ToList();

                var paged = new PagedResult<ChatSessionDto>
                {
                    Items = dtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = total,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize)
                };

                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, paged);
            }
            else
            {
                var query = _unitOfWork.ChatSessionRepository.GetAllQueryable().OrderByDescending(s => s.CreatedAt);
                var totalItems = await query.CountAsync();
                if (totalItems == 0)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                var sessions = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var paged = new PagedResult<ChatSessionDto>
                {
                    Items = sessions.Select(s => s.ToDto()).ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                };

                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, paged);
            }
        }

        public async Task<IServiceResult> UpdateSessionAsync(Guid sessionId, ChatSessionUpdateDto dto)
        {
            var session = await _unitOfWork.ChatSessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Session not found");
            }

            if (!string.IsNullOrWhiteSpace(dto.Status) && !AllowedStatuses.Contains(dto.Status))
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Invalid session status");
            }

            session.ApplyUpdate(dto);
            await _unitOfWork.ChatSessionRepository.UpdateAsync(session);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, session.ToDto());
        }
    }
}
