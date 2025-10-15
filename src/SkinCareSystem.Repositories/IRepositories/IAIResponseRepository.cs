using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IAIResponseRepository : IGenericRepository<Airesponse>
    {
        Task<IReadOnlyList<Airesponse>> GetByQueryAsync(Guid queryId);
        Task<IReadOnlyList<Airesponse>> GetByQueryIdsAsync(IEnumerable<Guid> queryIds);
    }
}
