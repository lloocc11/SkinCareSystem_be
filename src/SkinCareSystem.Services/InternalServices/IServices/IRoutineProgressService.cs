using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IRoutineProgressService
    {
        Task<IServiceResult> GetRoutineProgressByInstanceAsync(Guid instanceId, Guid requesterId, bool isAdmin);
        Task<IServiceResult> GetRoutineProgressByIdAsync(Guid progressId, Guid requesterId, bool isAdmin);
        Task<IServiceResult> GetRoutineProgressLogAsync(Guid instanceId, Guid requesterId, bool isAdmin);
        Task<IServiceResult> CreateRoutineProgressAsync(Guid requesterId, bool isAdmin, RoutineProgressCreateDto dto);
        Task<IServiceResult> UpdateRoutineProgressAsync(Guid progressId, Guid requesterId, bool isAdmin, RoutineProgressUpdateDto dto);
        Task<IServiceResult> DeleteRoutineProgressAsync(Guid progressId, Guid requesterId, bool isAdmin);
    }
}
