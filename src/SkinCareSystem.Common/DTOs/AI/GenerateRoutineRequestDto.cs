using System;
using System.Collections.Generic;

namespace SkinCareSystem.Common.DTOs.AI;

public class GenerateRoutineRequestDto
{
    public string Query { get; set; } = string.Empty;
    public string? TargetSkinType { get; set; }
    public IList<string> TargetConditions { get; set; } = new List<string>();
    /// <summary>
    /// Ngữ cảnh thô bổ sung (ví dụ nội dung file upload) để đưa vào prompt.
    /// </summary>
    public string? AdditionalContext { get; set; }
    public int K { get; set; } = 12;
    public int MaxSteps { get; set; } = 12;
    public int NumVariants { get; set; } = 1;
    public bool AutoSaveAsDraft { get; set; } = true;
    public IList<Guid>? DocumentIds { get; set; }
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
}
