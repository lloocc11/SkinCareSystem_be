using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public ChatSessionsController(IChatSessionService chatSessionService)
        {
            _chatSessionService = chatSessionService ?? throw new ArgumentNullException(nameof(chatSessionService));
        }

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

            var result = await _chatSessionService.CreateSessionAsync(dto);
            var location = result.Data is ChatSessionDto created ? $"/api/chat/sessions/{created.SessionId}" : null;
            return ToHttpResponse(result, location);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] Guid? userId = null, [FromQuery] bool mine = false, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            Guid? effectiveUserId = userId;
            if (mine)
            {
                var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(identityUserId, out var parsed))
                {
                    return ToHttpResponse(new ServiceResult
                    {
                        Status = Const.UNAUTHORIZED_ACCESS_CODE,
                        Message = Const.UNAUTHORIZED_ACCESS_MSG
                    });
                }

                effectiveUserId = parsed;
            }

            var result = await _chatSessionService.GetSessionsAsync(effectiveUserId, pageNumber, pageSize);
            return ToHttpResponse(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool includeMessages = false)
        {
            var result = await _chatSessionService.GetSessionAsync(id, includeMessages);
            return ToHttpResponse(result);
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ChatSessionUpdateDto dto)
        {
            var result = await _chatSessionService.UpdateSessionAsync(id, dto);
            return ToHttpResponse(result);
        }
    }
}
