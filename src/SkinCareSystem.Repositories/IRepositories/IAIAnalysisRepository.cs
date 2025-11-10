using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IAIAnalysisRepository : IGenericRepository<AIAnalysis>
    {
        Task<AIAnalysis?> GetByMessageIdAsync(Guid messageId);
        Task<IReadOnlyList<AIAnalysis>> GetBySessionAsync(Guid sessionId);
    }
}
