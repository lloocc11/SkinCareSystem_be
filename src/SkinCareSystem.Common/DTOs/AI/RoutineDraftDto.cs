using System.Collections.Generic;

namespace SkinCareSystem.Common.DTOs.AI;

public class RoutineDraftDto
{
    public string Description { get; set; } = string.Empty;
    public string? TargetSkinType { get; set; }
    public IList<string> TargetConditions { get; set; } = new List<string>();
    public IList<RoutineStepDraftDto> Steps { get; set; } = new List<RoutineStepDraftDto>();
    public bool IsRagBased { get; set; }
    public string Source { get; set; } = "llm";
}
