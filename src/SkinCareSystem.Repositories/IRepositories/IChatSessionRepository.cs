using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IChatSessionRepository : IGenericRepository<ChatSession>
    {
        Task<ChatSession?> GetByIdAsync(Guid sessionId, bool includeMessages = false);
        Task<IReadOnlyList<ChatSession>> GetByUserAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> CountByUserAsync(Guid userId);
    }
}
