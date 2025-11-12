using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SkinCareSystem.Common.DTOs.AI;

public class GenerateRoutineFormRequestDto
{
    public string? Query { get; set; }
    public string? TargetSkinType { get; set; }
    public List<string> TargetConditions { get; set; } = new();
    public int? MaxSteps { get; set; }
    public int? NumVariants { get; set; }
    public bool? AutoSaveAsDraft { get; set; }
    public List<IFormFile> Images { get; set; } = new();
}
