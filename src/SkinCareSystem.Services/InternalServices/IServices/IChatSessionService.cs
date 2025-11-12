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
        Task<IServiceResult> DeleteSessionAsync(Guid sessionId);
        Task<IServiceResult> GetWaitingSpecialistSessionsAsync(int pageNumber, int pageSize);
        Task<IServiceResult> GetAssignedSessionsAsync(Guid specialistId, int pageNumber, int pageSize);
        Task<IServiceResult> AssignSessionAsync(Guid sessionId, Guid specialistId, bool allowOverride);
        Task<IServiceResult> CloseSessionAsync(Guid sessionId, Guid requesterId, bool isAdmin, bool isSpecialist);
    }
}
