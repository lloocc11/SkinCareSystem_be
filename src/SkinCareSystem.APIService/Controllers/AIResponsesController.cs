using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.DTOs.AI;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing AI response operations.
    /// </summary>
    [Route("api/ai/responses")]
    [Authorize]
    public class AIResponsesController : BaseApiController
    {
        private readonly IAIResponseService _responseService;

        public AIResponsesController(IAIResponseService responseService)
        {
            _responseService = responseService ?? throw new ArgumentNullException(nameof(responseService));
        }
        /// <summary>
        /// POST /api/ai/responses - Create a new AI response record
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AIResponseCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _responseService.CreateResponseAsync(dto);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// GET /api/ai/responses/{responseId} - Get AI response by response ID
        /// </summary>
        /// <param name="responseId"></param>
        /// <returns></returns>
        [HttpGet("{responseId:guid}")]
        public async Task<IActionResult> Get(Guid responseId)
        {
            var result = await _responseService.GetResponseAsync(responseId);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// GET /api/ai/responses/query/{queryId} - Get AI responses by query ID
        /// </summary>
        /// <param name="queryId"></param>
        /// <returns></returns>
        [HttpGet("query/{queryId:guid}")]
        public async Task<IActionResult> GetByQuery(Guid queryId)
        {
            var result = await _responseService.GetResponsesByQueryAsync(queryId);
            return ToHttpResponse(result);
        }
    }
}
