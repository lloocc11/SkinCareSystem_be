using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IChatSessionService
    {
        Task<IServiceResult> CreateSessionAsync(ChatSessionCreateDto dto);
        Task<IServiceResult> GetSessionAsync(Guid sessionId, bool includeMessages = false);
        Task<IServiceResult> GetSessionsAsync(Guid? userId, int pageNumber, int pageSize);
        Task<IServiceResult> UpdateSessionAsync(Guid sessionId, ChatSessionUpdateDto dto);
    }
}
