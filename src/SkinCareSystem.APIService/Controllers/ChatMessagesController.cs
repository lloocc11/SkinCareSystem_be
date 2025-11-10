using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    [Route("api/chat")]
    [Authorize]
    public class ChatMessagesController : BaseApiController
    {
        private readonly IAiChatService _aiChatService;
        private readonly IChatMessageService _chatMessageService;
        private readonly IChatSessionService _chatSessionService;
        private readonly ILogger<ChatMessagesController> _logger;

        public ChatMessagesController(
            IAiChatService aiChatService,
            IChatMessageService chatMessageService,
            IChatSessionService chatSessionService,
            ILogger<ChatMessagesController> logger)
        {
            _aiChatService = aiChatService ?? throw new ArgumentNullException(nameof(aiChatService));
            _chatMessageService = chatMessageService ?? throw new ArgumentNullException(nameof(chatMessageService));
            _chatSessionService = chatSessionService ?? throw new ArgumentNullException(nameof(chatSessionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gửi tin nhắn tới AI trong chat session (có thể kèm ảnh).
        /// </summary>
        [HttpPost("sessions/{sessionId:guid}/messages")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<IActionResult> SendMessage(Guid sessionId, [FromForm] ChatMessageCreateDto dto)
        {
            if (!TryGetRequester(out var requesterId, out var unauthorizedResult))
            {
                return unauthorizedResult!;
            }

            var isAdmin = User.IsInRole("admin");
            var isSpecialist = User.IsInRole("specialist");

            dto.SessionId = sessionId;

            var (accessError, sessionDto) = await EnsureSessionAccessAsync(sessionId, requesterId, isAdmin, isSpecialist).ConfigureAwait(false);
            if (accessError != null)
            {
                return ToHttpResponse(accessError);
            }

            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE, Const.ERROR_INVALID_DATA_MSG));
            }

            if (isSpecialist && !isAdmin)
            {
                dto.UserId = requesterId;
                var specialistResult = await _chatMessageService.CreateMessageAsync(dto).ConfigureAwait(false);
                return ToHttpResponse(specialistResult);
            }

            dto.UserId = isAdmin ? sessionDto!.UserId : requesterId;

            var result = await _aiChatService.ChatInSessionAsync(dto).ConfigureAwait(false);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Lấy danh sách tin nhắn trong một session.
        /// </summary>
        [HttpGet("sessions/{sessionId:guid}/messages")]
        public async Task<IActionResult> GetMessages(Guid sessionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            if (!TryGetRequester(out var requesterId, out var unauthorizedResult))
            {
                return unauthorizedResult!;
            }

            var isAdmin = User.IsInRole("admin");
            var (accessError, _) = await EnsureSessionAccessAsync(sessionId, requesterId, isAdmin, User.IsInRole("specialist")).ConfigureAwait(false);
            if (accessError != null)
            {
                return ToHttpResponse(accessError);
            }

            var result = await _chatMessageService.GetMessagesBySessionAsync(sessionId, pageNumber, pageSize).ConfigureAwait(false);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Lấy chi tiết một tin nhắn (sử dụng cho audit/debug).
        /// </summary>
        [HttpGet("messages/{messageId:guid}")]
        public async Task<IActionResult> GetMessage(Guid messageId)
        {
            var result = await _chatMessageService.GetMessageAsync(messageId).ConfigureAwait(false);
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

        private async Task<(ServiceResult? Error, ChatSessionDto? Session)> EnsureSessionAccessAsync(Guid sessionId, Guid requesterId, bool isAdmin, bool isSpecialist)
        {
            var sessionResult = await _chatSessionService.GetSessionAsync(sessionId, includeMessages: false).ConfigureAwait(false);
            if (sessionResult.Status != Const.SUCCESS_READ_CODE || sessionResult.Data is not ChatSessionDto sessionDto)
            {
                var error = sessionResult as ServiceResult ?? new ServiceResult(sessionResult.Status, sessionResult.Message ?? string.Empty);
                return (error, null);
            }

            if (!isAdmin && !isSpecialist && sessionDto.UserId != requesterId)
            {
                return (new ServiceResult(Const.FORBIDDEN_ACCESS_CODE, Const.FORBIDDEN_ACCESS_MSG), null);
            }

            return (null, sessionDto);
        }
    }
}
