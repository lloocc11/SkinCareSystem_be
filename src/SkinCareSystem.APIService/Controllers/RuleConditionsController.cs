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
    /// Controller for managing rule conditions
    /// </summary>
    [Route("api/rule-conditions")]
    [Authorize]
    public class RuleConditionsController : BaseApiController
    {
        private readonly IRuleConditionService _ruleConditionService;

        public RuleConditionsController(IRuleConditionService ruleConditionService)
        {
            _ruleConditionService = ruleConditionService ?? throw new ArgumentNullException(nameof(ruleConditionService));
        }

        /// <summary>
        /// Get all conditions for a specific rule
        /// </summary>
        [HttpGet("rule/{ruleId:guid}")]
        public async Task<IActionResult> GetByRule(Guid ruleId)
        {
            var result = await _ruleConditionService.GetRuleConditionsByRuleAsync(ruleId);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Get a specific rule condition by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _ruleConditionService.GetRuleConditionByIdAsync(id);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Create a new rule condition
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] RuleConditionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _ruleConditionService.CreateRuleConditionAsync(dto);
            var location = result.Data is RuleConditionDto created ? $"/api/rule-conditions/{created.RuleConditionId}" : null;
            return ToHttpResponse(result, location);
        }

        /// <summary>
        /// Update an existing rule condition
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RuleConditionUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _ruleConditionService.UpdateRuleConditionAsync(id, dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Delete a rule condition
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _ruleConditionService.DeleteRuleConditionAsync(id);
            return ToHttpResponse(result);
        }
    }
}
