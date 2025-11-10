using System.Collections.Generic;

namespace SkinCareSystem.Common.DTOs.AI;

public class GenerateRoutineFromUploadRequestDto
{
    public string? Query { get; set; }

    public string? TargetSkinType { get; set; }

    /// <summary>
    /// Bind bằng cách gửi nhiều field targetConditions trong form-data hoặc truyền mảng.
    /// </summary>
    public List<string> TargetConditions { get; set; } = new();

    public bool AutoSaveAsDraft { get; set; } = true;
}
