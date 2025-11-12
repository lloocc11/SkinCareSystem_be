using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SkinCareSystem.Common.DTOs.AI;

public class GenerateRoutineFromTextFormRequestDto
{
    public string? Prompt { get; set; }
    public string? Context { get; set; }
    public string? TargetSkinType { get; set; }
    public List<string> TargetConditions { get; set; } = new();
    public bool? AutoSaveAsDraft { get; set; }
    public List<IFormFile> Images { get; set; } = new();
}
