using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.DTOs.Rule;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing skincare recommendation rules
    /// </summary>
    [Route("api/rules")]
    [Authorize]
    public class RulesController : BaseApiController
    {
        private readonly IRuleService _ruleService;

        public RulesController(IRuleService ruleService)
        {
            _ruleService = ruleService ?? throw new ArgumentNullException(nameof(ruleService));
        }

        /// <summary>
        /// Get all rules with pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetRules([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _ruleService.GetRulesAsync(pageNumber, pageSize);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Get a specific rule by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _ruleService.GetRuleByIdAsync(id);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Create a new rule
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] RuleCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _ruleService.CreateRuleAsync(dto);
            var location = result.Data is RuleDto created ? $"/api/rules/{created.RuleId}" : null;
            return ToHttpResponse(result, location);
        }

        /// <summary>
        /// Update an existing rule
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RuleUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _ruleService.UpdateRuleAsync(id, dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Delete a rule
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _ruleService.DeleteRuleAsync(id);
            return ToHttpResponse(result);
        }
    }
}
