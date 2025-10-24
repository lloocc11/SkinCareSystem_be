using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.IRepositories;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.Repositories
{
    public class RuleConditionRepository : GenericRepository<RuleCondition>, IRuleConditionRepository
    {
        public RuleConditionRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<RuleCondition>> GetByRuleIdAsync(Guid ruleId)
        {
            return await _context.Set<RuleCondition>()
                .AsNoTracking()
                .Include(rc => rc.Rule)
                .Include(rc => rc.Symptom)
                .Include(rc => rc.Question)
                .Where(rc => rc.RuleId == ruleId)
                .ToListAsync();
        }

        public async Task<RuleCondition?> GetByIdWithDetailsAsync(Guid ruleConditionId)
        {
            return await _context.Set<RuleCondition>()
                .AsNoTracking()
                .Include(rc => rc.Rule)
                .Include(rc => rc.Symptom)
                .Include(rc => rc.Question)
                .FirstOrDefaultAsync(rc => rc.RuleConditionId == ruleConditionId);
        }
    }
}
