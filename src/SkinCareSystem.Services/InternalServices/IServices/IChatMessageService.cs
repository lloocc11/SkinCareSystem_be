using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IChatMessageService
    {
        Task<IServiceResult> CreateMessageAsync(ChatMessageCreateDto dto);
        Task<IServiceResult> UploadImageMessageAsync(Guid sessionId, Guid userId, string role, IFormFile file, string? messageType = null);
        Task<IServiceResult> GetMessageAsync(Guid messageId);
        Task<IServiceResult> GetMessagesBySessionAsync(Guid sessionId, int pageNumber, int pageSize);
    }
}
