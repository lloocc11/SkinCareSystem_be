using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Common.Constants;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Common.DTOs.Pagination;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class ChatSessionService : IChatSessionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatSessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> CreateSessionAsync(ChatSessionCreateDto dto)
        {
            if (!ChatSessionChannels.TryNormalize(dto.Channel, out var normalizedChannel))
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE,
                    $"channel must be one of: {string.Join(", ", ChatSessionChannels.Values)}");
            }

            dto.Channel = normalizedChannel;

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
                Session = session.ToDto(),
                Messages = session.ChatMessages.Select(m => m.ToDto()).ToList()
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

                var sessions = await _unitOfWork.ChatSessionRepository
                    .GetByUserAsync(userId.Value, pageNumber, pageSize);
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
                var query = _unitOfWork.ChatSessionRepository
                    .GetAllQueryable()
                    .AsNoTracking()
                    .OrderByDescending(s => s.created_at);
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

            session.ApplyUpdate(dto);
            await _unitOfWork.ChatSessionRepository.UpdateAsync(session);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, session.ToDto());
        }

        public async Task<IServiceResult> DeleteSessionAsync(Guid sessionId)
        {
            var session = await _unitOfWork.ChatSessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Session not found");
            }

            await _unitOfWork.ChatSessionRepository.RemoveAsync(session);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
        }

        public async Task<IServiceResult> GetWaitingSpecialistSessionsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, Const.ERROR_INVALID_DATA_MSG);
            }

            var query = _unitOfWork.ChatSessionRepository
                .GetAllQueryable()
                .AsNoTracking()
                .Where(s =>
                    s.channel == ChatSessionChannels.Specialist &&
                    s.state == ChatSessionStates.WaitingSpecialist)
                .OrderBy(s => s.created_at);

            var total = await query.CountAsync();
            if (total == 0)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "No specialist sessions waiting.");
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
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, paged);
        }

        public async Task<IServiceResult> GetAssignedSessionsAsync(Guid specialistId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, Const.ERROR_INVALID_DATA_MSG);
            }

            var query = _unitOfWork.ChatSessionRepository
                .GetAllQueryable()
                .AsNoTracking()
                .Where(s =>
                    s.channel == ChatSessionChannels.Specialist &&
                    s.state == ChatSessionStates.Assigned &&
                    s.specialist_id == specialistId)
                .OrderByDescending(s => s.assigned_at ?? s.updated_at ?? s.created_at);

            var total = await query.CountAsync();
            if (total == 0)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "No assigned sessions found.");
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
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, paged);
        }

        public async Task<IServiceResult> AssignSessionAsync(Guid sessionId, Guid specialistId, bool allowOverride)
        {
            var session = await _unitOfWork.ChatSessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Session not found");
            }

            if (!string.Equals(session.channel, ChatSessionChannels.Specialist, StringComparison.OrdinalIgnoreCase))
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Only specialist channel sessions can be assigned.");
            }

            if (string.Equals(session.state, ChatSessionStates.Closed, StringComparison.OrdinalIgnoreCase))
            {
                return new ServiceResult(Const.WARNING_DATA_EXISTED_CODE, "Session is already closed.");
            }

            if (string.Equals(session.state, ChatSessionStates.Assigned, StringComparison.OrdinalIgnoreCase))
            {
                if (session.specialist_id == specialistId)
                {
                    return new ServiceResult(Const.SUCCESS_UPDATE_CODE, "Session already assigned to you.", session.ToDto());
                }

                if (!allowOverride)
                {
                    return new ServiceResult(Const.WARNING_DATA_EXISTED_CODE, "Session has been claimed by another specialist.");
                }
            }
            else if (!string.Equals(session.state, ChatSessionStates.WaitingSpecialist, StringComparison.OrdinalIgnoreCase))
            {
                return new ServiceResult(Const.WARNING_DATA_EXISTED_CODE, "Session is not waiting for assignment.");
            }

            session.specialist_id = specialistId;
            session.assigned_at = DateTimeHelper.UtcNowUnspecified();
            session.state = ChatSessionStates.Assigned;
            session.updated_at = session.assigned_at;

            await _unitOfWork.ChatSessionRepository.UpdateAsync(session);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(Const.SUCCESS_UPDATE_CODE, "Session assigned.", session.ToDto());
        }

        public async Task<IServiceResult> CloseSessionAsync(Guid sessionId, Guid requesterId, bool isAdmin, bool isSpecialist)
        {
            var session = await _unitOfWork.ChatSessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Session not found");
            }

            if (string.Equals(session.state, ChatSessionStates.Closed, StringComparison.OrdinalIgnoreCase))
            {
                return new ServiceResult(Const.WARNING_DATA_EXISTED_CODE, "Session is already closed.");
            }

            var canClose = false;

            if (isAdmin)
            {
                canClose = true;
            }
            else if (session.user_id == requesterId)
            {
                canClose = true;
            }
            else if (isSpecialist &&
                     string.Equals(session.channel, ChatSessionChannels.Specialist, StringComparison.OrdinalIgnoreCase) &&
                     session.specialist_id == requesterId)
            {
                canClose = true;
            }

            if (!canClose)
            {
                return new ServiceResult(Const.FORBIDDEN_ACCESS_CODE, Const.FORBIDDEN_ACCESS_MSG);
            }

            if (string.Equals(session.state, ChatSessionStates.WaitingSpecialist, StringComparison.OrdinalIgnoreCase))
            {
                return new ServiceResult(Const.WARNING_DATA_EXISTED_CODE, "Session must be assigned before closing.");
            }

            var now = DateTimeHelper.UtcNowUnspecified();
            session.state = ChatSessionStates.Closed;
            session.closed_at = now;
            session.updated_at = now;

            await _unitOfWork.ChatSessionRepository.UpdateAsync(session);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(Const.SUCCESS_UPDATE_CODE, "Session closed.", session.ToDto());
        }
    }
}
