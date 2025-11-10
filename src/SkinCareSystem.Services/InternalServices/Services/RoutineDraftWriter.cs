using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkinCareSystem.Common.DTOs.AI;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.Services.InternalServices.Services;

public class RoutineDraftWriter : IRoutineDraftWriter
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoutineDraftWriter> _logger;

    public RoutineDraftWriter(IUnitOfWork unitOfWork, ILogger<RoutineDraftWriter> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid?> SaveDraftAsync(RoutineDraftDto draft, Guid creatorUserId, string? targetSkinType, IList<string> targetConditions)
    {
        if (draft == null)
        {
            return null;
        }

        if (creatorUserId == Guid.Empty)
        {
            throw new ArgumentException("CreatorUserId is required.", nameof(creatorUserId));
        }

        var now = DateTimeHelper.UtcNowUnspecified();
        var routine = new Routine
        {
            routine_id = Guid.NewGuid(),
            user_id = creatorUserId,
            description = draft.Description,
            target_skin_type = targetSkinType ?? draft.TargetSkinType,
            target_conditions = NormalizeConditions(targetConditions?.Count > 0
                ? targetConditions
                : draft.TargetConditions),
            routine_type = "template",
            status = "draft",
            created_at = now,
            updated_at = now
        };

        await _unitOfWork.RoutineRepository.CreateAsync(routine);

        var steps = draft.Steps?.Count > 0
            ? draft.Steps
            : new List<RoutineStepDraftDto> { new RoutineStepDraftDto { Order = 1, Instruction = "Routine bước cơ bản", TimeOfDay = "morning", Frequency = "daily" } };

        foreach (var step in steps.OrderBy(s => s.Order))
        {
            var entity = new RoutineStep
            {
                step_id = Guid.NewGuid(),
                routine_id = routine.routine_id,
                step_order = step.Order <= 0 ? 1 : step.Order,
                instruction = step.Instruction,
                time_of_day = string.IsNullOrWhiteSpace(step.TimeOfDay) ? "morning" : step.TimeOfDay,
                frequency = string.IsNullOrWhiteSpace(step.Frequency) ? "daily" : step.Frequency
            };

            await _unitOfWork.RoutineStepRepository.CreateAsync(entity);
        }

        try
        {
            await _unitOfWork.SaveAsync();
            return routine.routine_id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist AI routine draft.");
            return null;
        }
    }

    private static string? NormalizeConditions(IList<string>? conditions)
    {
        if (conditions == null || conditions.Count == 0)
        {
            return null;
        }

        return string.Join(", ",
            conditions
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim()));
    }
}
