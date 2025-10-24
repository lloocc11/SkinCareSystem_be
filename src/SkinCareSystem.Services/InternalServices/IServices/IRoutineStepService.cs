using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IRoutineStepService
    {
        Task<IServiceResult> GetRoutineStepsByRoutineAsync(Guid routineId);
        Task<IServiceResult> GetRoutineStepByIdAsync(Guid stepId);
        Task<IServiceResult> CreateRoutineStepAsync(RoutineStepCreateDto dto);
        Task<IServiceResult> UpdateRoutineStepAsync(Guid stepId, RoutineStepUpdateDto dto);
        Task<IServiceResult> DeleteRoutineStepAsync(Guid stepId);
    }
}
