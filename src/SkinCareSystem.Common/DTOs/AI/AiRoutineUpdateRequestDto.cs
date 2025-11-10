using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.AI;

public class AiRoutineUpdateRequestDto
{
    [MaxLength(2048)]
    public string? Description { get; set; }

    [MaxLength(64)]
    public string? TargetSkinType { get; set; }

    public IList<string>? TargetConditions { get; set; }

    public IList<RoutineStepDraftDto>? Steps { get; set; }

    /// <summary>
    /// Optional status override (draft/published/archived). Defaults to existing status.
    /// </summary>
    [MaxLength(32)]
    public string? Status { get; set; }
}
