using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace SkinCareSystem.APIService.Controllers
{
    [Route("api/chat")]
    [Authorize]
    /// <summary>
    /// Controller for managing chat messages.
    /// </summary>
    public class ChatMessagesController : BaseApiController
    {
        private readonly IChatMessageService _chatMessageService;

        public ChatMessagesController(IChatMessageService chatMessageService)
        {
            _chatMessageService = chatMessageService ?? throw new ArgumentNullException(nameof(chatMessageService));
        }
        /// <summary>
        /// POST /api/chat/sessions/{sessionId}/messages - Create a new chat message
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("sessions/{sessionId:guid}/messages")]
        public async Task<IActionResult> CreateMessage(Guid sessionId, [FromBody] ChatMessageCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            dto.SessionId = sessionId;
            var result = await _chatMessageService.CreateMessageAsync(dto);
            var location = result.Data is ChatMessageDto messageDto ? $"/api/chat/messages/{messageDto.MessageId}" : null;
            return ToHttpResponse(result, location);
        }
        /// <summary>
        /// GET /api/chat/sessions/{sessionId}/messages - Get chat messages by session ID
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("sessions/{sessionId:guid}/messages")]
        public async Task<IActionResult> GetMessages(Guid sessionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _chatMessageService.GetMessagesBySessionAsync(sessionId, pageNumber, pageSize);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// GET /api/chat/messages/{messageId} - Get chat message by message ID
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [HttpGet("messages/{messageId:guid}")]
        public async Task<IActionResult> GetMessage(Guid messageId)
        {
            var result = await _chatMessageService.GetMessageAsync(messageId);
            return ToHttpResponse(result);
        }
    }
}
