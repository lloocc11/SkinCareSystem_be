using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IRuleConditionRepository : IGenericRepository<RuleCondition>
    {
        Task<IReadOnlyList<RuleCondition>> GetByRuleIdAsync(Guid ruleId);
        Task<RuleCondition?> GetByIdWithDetailsAsync(Guid ruleConditionId);
    }
}
