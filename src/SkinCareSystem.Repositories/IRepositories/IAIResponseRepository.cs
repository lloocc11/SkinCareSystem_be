using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IAIResponseRepository : IGenericRepository<AIResponse>
    {
        Task<IReadOnlyList<AIResponse>> GetByQueryAsync(Guid queryId);
        Task<IReadOnlyList<AIResponse>> GetByQueryIdsAsync(IEnumerable<Guid> queryIds);
    }
}
