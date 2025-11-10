using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.IRepositories;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.Repositories
{
    public class RuleRepository : GenericRepository<Rule>, IRuleRepository
    {
        public RuleRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<Rule?> GetByIdWithDetailsAsync(Guid ruleId)
        {
            return await _context.Set<Rule>()
                .AsNoTracking()
                .Include(r => r.RuleConditions)
                .FirstOrDefaultAsync(r => r.rule_id == ruleId);
        }
    }
}
