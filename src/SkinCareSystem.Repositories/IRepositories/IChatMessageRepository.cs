using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IChatMessageRepository : IGenericRepository<ChatMessage>
    {
        Task<IReadOnlyList<ChatMessage>> GetBySessionAsync(Guid sessionId, int pageNumber, int pageSize);
        Task<int> CountBySessionAsync(Guid sessionId);
    }
}
