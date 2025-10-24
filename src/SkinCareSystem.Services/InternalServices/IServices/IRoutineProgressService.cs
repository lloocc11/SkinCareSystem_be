using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IRoutineProgressService
    {
        Task<IServiceResult> GetRoutineProgressByInstanceAsync(Guid instanceId);
        Task<IServiceResult> GetRoutineProgressByIdAsync(Guid progressId);
        Task<IServiceResult> CreateRoutineProgressAsync(RoutineProgressCreateDto dto);
        Task<IServiceResult> UpdateRoutineProgressAsync(Guid progressId, RoutineProgressUpdateDto dto);
        Task<IServiceResult> DeleteRoutineProgressAsync(Guid progressId);
    }
}
