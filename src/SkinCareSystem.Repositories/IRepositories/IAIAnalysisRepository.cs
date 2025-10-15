using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IAIAnalysisRepository : IGenericRepository<Aianalysis>
    {
        Task<Aianalysis?> GetByMessageIdAsync(Guid messageId);
        Task<IReadOnlyList<Aianalysis>> GetBySessionAsync(Guid sessionId);
    }
}
