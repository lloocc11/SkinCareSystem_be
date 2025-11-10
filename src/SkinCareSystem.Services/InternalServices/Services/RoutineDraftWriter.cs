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
                time_of_day = NormalizeTimeOfDay(step.TimeOfDay),
                frequency = NormalizeFrequency(step.Frequency)
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

    private static string NormalizeFrequency(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "daily";
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Contains("weekly") || normalized.Contains("tuần"))
        {
            return "weekly";
        }

        if (normalized.Contains("twice") || normalized.Contains("2 lần") || normalized.Contains("hai lần") || normalized.Contains("sáng và tối"))
        {
            return "twice_daily";
        }

        if (normalized.Contains("khi cần") || normalized.Contains("as needed") || normalized.Contains("tùy"))
        {
            return "as_needed";
        }

        if (normalized.Contains("daily") || normalized.Contains("mỗi ngày") || normalized.Contains("hằng ngày"))
        {
            return "daily";
        }

        return "daily";
    }

    private static string NormalizeTimeOfDay(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "morning";
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Contains("sáng") && normalized.Contains("tối"))
        {
            return "both";
        }

        if (normalized.Contains("both") || normalized.Contains("cả ngày"))
        {
            return "both";
        }

        if (normalized.Contains("tối") || normalized.Contains("evening") || normalized.Contains("night"))
        {
            return "evening";
        }

        return normalized.Contains("morning") || normalized.Contains("sáng")
            ? "morning"
            : "morning";
    }
}
