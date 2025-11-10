using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IRoutineInstanceService
    {
        Task<IServiceResult> GetRoutineInstancesByUserAsync(Guid userId, int pageNumber, int pageSize);
        Task<IServiceResult> GetRoutineInstancesByRoutineAsync(Guid routineId);
        Task<IServiceResult> GetRoutineInstanceByIdAsync(Guid instanceId);
        Task<IServiceResult> CreateRoutineInstanceAsync(RoutineInstanceCreateDto dto);
        Task<IServiceResult> UpdateRoutineInstanceAsync(Guid instanceId, RoutineInstanceUpdateDto dto);
        Task<IServiceResult> DeleteRoutineInstanceAsync(Guid instanceId);
    }
}
