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
    /// Controller for managing AI Analysis operations.
    /// </summary>
    [Route("api/ai/analysis")]
    [Authorize]
    public class AIAnalysisController : BaseApiController
    {
        private readonly IAIAnalysisService _analysisService;

        public AIAnalysisController(IAIAnalysisService analysisService)
        {
            _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        }
        /// <summary>
        /// POST /api/ai/analysis - Create a new AI analysis record
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AIAnalysisCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _analysisService.CreateAnalysisAsync(dto);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// GET /api/ai/analysis/message/{messageId} - Get AI analysis by message ID
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [HttpGet("message/{messageId:guid}")]
        public async Task<IActionResult> GetByMessage(Guid messageId)
        {
            var result = await _analysisService.GetAnalysisByMessageAsync(messageId);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// GET /api/ai/analysis/session/{sessionId} - Get AI analyses by session ID
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        [HttpGet("session/{sessionId:guid}")]
        public async Task<IActionResult> GetBySession(Guid sessionId)
        {
            var result = await _analysisService.GetAnalysesBySessionAsync(sessionId);
            return ToHttpResponse(result);
        }
    }
}
