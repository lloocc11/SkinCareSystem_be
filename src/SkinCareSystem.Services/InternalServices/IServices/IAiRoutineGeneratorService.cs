using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.AI;

namespace SkinCareSystem.Services.InternalServices.IServices;

public interface IAiRoutineGeneratorService
{
    Task<RoutineDraftDto> GenerateAsync(GenerateRoutineRequestDto request);
}
