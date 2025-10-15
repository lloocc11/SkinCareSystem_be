using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.AI;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IAIResponseService
    {
        Task<IServiceResult> CreateResponseAsync(AIResponseCreateDto dto);
        Task<IServiceResult> GetResponseAsync(Guid responseId);
        Task<IServiceResult> GetResponsesByQueryAsync(Guid queryId);
    }
}
