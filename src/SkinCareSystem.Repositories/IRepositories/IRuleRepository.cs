using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IRuleRepository : IGenericRepository<Rule>
    {
        Task<Rule?> GetByIdWithDetailsAsync(Guid ruleId);
    }
}
