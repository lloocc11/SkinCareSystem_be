using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IRoutineService
    {
        Task<IServiceResult> GetRoutinesAsync(int pageNumber, int pageSize);
        Task<IServiceResult> GetRoutinesByUserAsync(Guid userId, int pageNumber, int pageSize);
        Task<IServiceResult> GetRoutineByIdAsync(Guid routineId);
        Task<IServiceResult> CreateRoutineAsync(RoutineCreateDto dto);
        Task<IServiceResult> UpdateRoutineAsync(Guid routineId, RoutineUpdateDto dto);
        Task<IServiceResult> DeleteRoutineAsync(Guid routineId);
    }
}
