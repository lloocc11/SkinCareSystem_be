using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Rule;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IRuleService
    {
        Task<IServiceResult> GetRulesAsync(int pageNumber, int pageSize);
        Task<IServiceResult> GetRuleByIdAsync(Guid ruleId);
        Task<IServiceResult> CreateRuleAsync(RuleCreateDto dto);
        Task<IServiceResult> UpdateRuleAsync(Guid ruleId, RuleUpdateDto dto);
        Task<IServiceResult> DeleteRuleAsync(Guid ruleId);
    }

    public interface IRuleConditionService
    {
        Task<IServiceResult> GetRuleConditionsByRuleAsync(Guid ruleId);
        Task<IServiceResult> GetRuleConditionByIdAsync(Guid ruleConditionId);
        Task<IServiceResult> CreateRuleConditionAsync(RuleConditionCreateDto dto);
        Task<IServiceResult> UpdateRuleConditionAsync(Guid ruleConditionId, RuleConditionUpdateDto dto);
        Task<IServiceResult> DeleteRuleConditionAsync(Guid ruleConditionId);
    }
}
