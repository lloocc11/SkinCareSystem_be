using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkinCareSystem.Common.Constants;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    [Route("api/chat/sessions")]
    [Authorize]
    public class ChatSessionsController : BaseApiController
    {
        private readonly IChatSessionService _chatSessionService;
        private readonly ILogger<ChatSessionsController> _logger;

        public ChatSessionsController(IChatSessionService chatSessionService, ILogger<ChatSessionsController> logger)
        {
            _chatSessionService = chatSessionService ?? throw new ArgumentNullException(nameof(chatSessionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Tạo chat session mới để người dùng trao đổi với AI hoặc yêu cầu specialist.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ChatSessionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            if (!TryGetRequester(out var requesterId, out var unauthorizedResult))
            {
                return unauthorizedResult!;
            }

            var isAdmin = User.IsInRole("admin");
            var isSpecialist = User.IsInRole("specialist");

            if (!ChatSessionChannels.TryNormalize(dto.Channel, out var normalizedChannel))
            {
                return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE,
                    $"channel must be one of: {string.Join(", ", ChatSessionChannels.Values)}"));
            }

            dto.Channel = normalizedChannel;

            if (normalizedChannel == ChatSessionChannels.AiAdmin)
            {
                if (!isAdmin && !isSpecialist)
                {
                    return ToHttpResponse(new ServiceResult(Const.FORBIDDEN_ACCESS_CODE, Const.FORBIDDEN_ACCESS_MSG));
                }

                dto.UserId = requesterId;
            }
            else if (!isAdmin && requesterId != dto.UserId)
            {
                return ToHttpResponse(new ServiceResult(Const.FORBIDDEN_ACCESS_CODE, Const.FORBIDDEN_ACCESS_MSG));
            }

            if (dto.UserId == Guid.Empty)
            {
                return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE, "userId is required."));
            }

            var result = await _chatSessionService.CreateSessionAsync(dto).ConfigureAwait(false);
            var location = result.Data is ChatSessionDto created ? $"/api/chat/sessions/{created.SessionId}" : null;
            return ToHttpResponse(result, location);
        }

        /// <summary>
        /// Lấy chi tiết session theo Id (khi cần có thể gồm cả message).
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool includeMessages = false)
        {
            if (!TryGetRequester(out var requesterId, out var unauthorizedResult))
            {
                return unauthorizedResult!;
            }

            var isAdmin = User.IsInRole("admin");
            var isSpecialist = User.IsInRole("specialist");
            var securityResult = await _chatSessionService.GetSessionAsync(id, includeMessages: false).ConfigureAwait(false);
            if (securityResult.Status != Const.SUCCESS_READ_CODE)
            {
                return ToHttpResponse(securityResult);
            }

            if (securityResult.Data is ChatSessionDto sessionDto)
            {
                var isOwner = sessionDto.UserId == requesterId;
                var isAssignedSpecialist = sessionDto.SpecialistId.HasValue && sessionDto.SpecialistId == requesterId;

                var channel = sessionDto.Channel;
                var canRead = isAdmin
                              || isOwner
                              || (isSpecialist
                                  && string.Equals(channel, ChatSessionChannels.Specialist, StringComparison.OrdinalIgnoreCase)
                                  && isAssignedSpecialist);

                if (!canRead)
                {
                    return ToHttpResponse(new ServiceResult(Const.FORBIDDEN_ACCESS_CODE, Const.FORBIDDEN_ACCESS_MSG));
                }
            }
            else
            {
                _logger.LogWarning("Unexpected payload type when fetching session {SessionId}", id);
            }

            if (!includeMessages)
            {
                return ToHttpResponse(securityResult);
            }

            var fullResult = await _chatSessionService.GetSessionAsync(id, includeMessages: true).ConfigureAwait(false);
            return ToHttpResponse(fullResult);
        }

        /// <summary>
        /// Liệt kê những session của một user.
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            if (!TryGetRequester(out var requesterId, out var unauthorizedResult))
            {
                return unauthorizedResult!;
            }

            var isAdmin = User.IsInRole("admin");
            if (!isAdmin && requesterId != userId)
            {
                return ToHttpResponse(new ServiceResult(Const.FORBIDDEN_ACCESS_CODE, Const.FORBIDDEN_ACCESS_MSG));
            }

            var result = await _chatSessionService.GetSessionsAsync(userId, pageNumber, pageSize).ConfigureAwait(false);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Liệt kê các phiên dành cho specialist (hàng chờ hoặc phiên đã gán cho chính mình).
        /// </summary>
        [HttpGet("~/api/chat/specialist-sessions")]
        [Authorize(Roles = "admin,specialist")]
        public async Task<IActionResult> GetSpecialistSessions(
            [FromQuery] string state = "waiting_specialist",
            [FromQuery] bool mine = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (!TryGetRequester(out var requesterId, out var unauthorizedResult))
            {
                return unauthorizedResult!;
            }

            var normalizedState = (state ?? ChatSessionStates.WaitingSpecialist).Trim().ToLowerInvariant();

            if (normalizedState == ChatSessionStates.WaitingSpecialist)
            {
                var waiting = await _chatSessionService
                    .GetWaitingSpecialistSessionsAsync(pageNumber, pageSize)
                    .ConfigureAwait(false);
                return ToHttpResponse(waiting);
            }

            if (normalizedState == ChatSessionStates.Assigned)
            {
                if (!mine)
                {
                    return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE,
                        "scope 'mine' is required when requesting assigned sessions."));
                }

                var assigned = await _chatSessionService
                    .GetAssignedSessionsAsync(requesterId, pageNumber, pageSize)
                    .ConfigureAwait(false);
                return ToHttpResponse(assigned);
            }

            return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE,
                "Unsupported state. Use waiting_specialist or assigned."));
        }

        /// <summary>
        /// Specialist/admin claim một session đang chờ.
        /// </summary>
        [HttpPost("{sessionId:guid}/assignments")]
        [Authorize(Roles = "admin,specialist")]
        public async Task<IActionResult> AssignSession(Guid sessionId)
        {
            if (!TryGetRequester(out var requesterId, out var unauthorizedResult))
            {
                return unauthorizedResult!;
            }

            var isAdmin = User.IsInRole("admin");
            var result = await _chatSessionService.AssignSessionAsync(sessionId, requesterId, allowOverride: isAdmin).ConfigureAwait(false);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Đóng session (người dùng, specialist hoặc admin).
        /// </summary>
        [HttpPost("{sessionId:guid}/closures")]
        [Authorize]
        public async Task<IActionResult> CloseSession(Guid sessionId)
        {
            if (!TryGetRequester(out var requesterId, out var unauthorizedResult))
            {
                return unauthorizedResult!;
            }

            var result = await _chatSessionService.CloseSessionAsync(
                sessionId,
                requesterId,
                isAdmin: User.IsInRole("admin"),
                isSpecialist: User.IsInRole("specialist")).ConfigureAwait(false);

            return ToHttpResponse(result);
        }

        private bool TryGetRequester(out Guid requesterId, out IActionResult? errorResult)
        {
            requesterId = Guid.Empty;
            errorResult = null;

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out requesterId))
            {
                errorResult = ToHttpResponse(new ServiceResult(Const.UNAUTHORIZED_ACCESS_CODE, Const.UNAUTHORIZED_ACCESS_MSG));
                return false;
            }

            return true;
        }
    }
}
