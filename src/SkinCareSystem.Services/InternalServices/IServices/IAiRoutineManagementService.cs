using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.AI;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices;

public interface IAiRoutineManagementService
{
    Task<ServiceResult> PublishAsync(Guid routineId);
    Task<ServiceResult> ArchiveAsync(Guid routineId);
    Task<ServiceResult> UpdateAsync(Guid routineId, AiRoutineUpdateRequestDto request);
}
