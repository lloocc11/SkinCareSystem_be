using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.AI;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services;

public class AiRoutineManagementService : IAiRoutineManagementService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "draft", "published", "archived"
    };

    private readonly IUnitOfWork _unitOfWork;

    public AiRoutineManagementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ServiceResult> PublishAsync(Guid routineId)
    {
        var routine = await _unitOfWork.RoutineRepository.GetByIdAsync(routineId);
        if (routine == null)
        {
            return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Routine not found.");
        }

        routine.status = "published";
        routine.updated_at = DateTimeHelper.UtcNowUnspecified();

        await _unitOfWork.RoutineRepository.UpdateAsync(routine);
        await _unitOfWork.SaveAsync();

        var dto = (await _unitOfWork.RoutineRepository.GetByIdWithDetailsAsync(routineId))?.ToDto();
        return new ServiceResult(Const.SUCCESS_UPDATE_CODE, "Routine published.", dto!);
    }

    public async Task<ServiceResult> ArchiveAsync(Guid routineId)
    {
        var routine = await _unitOfWork.RoutineRepository.GetByIdAsync(routineId);
        if (routine == null)
        {
            return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Routine not found.");
        }

        routine.status = "archived";
        routine.updated_at = DateTimeHelper.UtcNowUnspecified();

        await _unitOfWork.RoutineRepository.UpdateAsync(routine);
        await _unitOfWork.SaveAsync();

        var dto = (await _unitOfWork.RoutineRepository.GetByIdWithDetailsAsync(routineId))?.ToDto();
        return new ServiceResult(Const.SUCCESS_UPDATE_CODE, "Routine archived.", dto!);
    }

    public async Task<ServiceResult> UpdateAsync(Guid routineId, AiRoutineUpdateRequestDto request)
    {
        var routine = await _unitOfWork.RoutineRepository.GetByIdAsync(routineId);
        if (routine == null)
        {
            return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Routine not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            routine.description = request.Description.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.TargetSkinType))
        {
            routine.target_skin_type = request.TargetSkinType.Trim();
        }

        if (request.TargetConditions != null)
        {
            routine.target_conditions = NormalizeConditions(request.TargetConditions);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var normalized = request.Status.Trim().ToLowerInvariant();
            if (!AllowedStatuses.Contains(normalized))
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, $"Invalid status '{request.Status}'.");
            }

            routine.status = normalized;
        }

        routine.updated_at = DateTimeHelper.UtcNowUnspecified();

        await _unitOfWork.RoutineRepository.UpdateAsync(routine);

        if (request.Steps != null)
        {
            await _unitOfWork.RoutineStepRepository.DeleteByRoutineIdAsync(routineId);

            var orderedSteps = request.Steps
                .Where(s => !string.IsNullOrWhiteSpace(s.Instruction))
                .OrderBy(s => s.Order <= 0 ? int.MaxValue : s.Order)
                .Select((s, index) => new RoutineStep
                {
                    step_id = Guid.NewGuid(),
                    routine_id = routineId,
                    step_order = index + 1,
                    instruction = s.Instruction.Trim(),
                    time_of_day = string.IsNullOrWhiteSpace(s.TimeOfDay) ? "unspecified" : s.TimeOfDay!,
                    frequency = string.IsNullOrWhiteSpace(s.Frequency) ? "daily" : s.Frequency!
                });

            foreach (var step in orderedSteps)
            {
                await _unitOfWork.RoutineStepRepository.CreateAsync(step);
            }
        }

        await _unitOfWork.SaveAsync();

        var dto = (await _unitOfWork.RoutineRepository.GetByIdWithDetailsAsync(routineId))?.ToDto();
        return new ServiceResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, dto!);
    }

    private static string? NormalizeConditions(IList<string>? conditions)
    {
        if (conditions == null || conditions.Count == 0)
        {
            return null;
        }

        var normalized = conditions
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim());

        var joined = string.Join(", ", normalized);
        return string.IsNullOrWhiteSpace(joined) ? null : joined;
    }
}
