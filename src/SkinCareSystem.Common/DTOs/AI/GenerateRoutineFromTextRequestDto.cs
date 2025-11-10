using System.Collections.Generic;

namespace SkinCareSystem.Common.DTOs.AI;

public class GenerateRoutineFromTextRequestDto
{
    /// <summary>
    /// Mô tả/nhu cầu chính của routine (dùng cho Query).
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Ngữ cảnh bổ sung (copy từ chat, hướng dẫn chi tiết, v.v.).
    /// </summary>
    public string? Context { get; set; }

    public string? TargetSkinType { get; set; }

    public List<string> TargetConditions { get; set; } = new();

    public bool AutoSaveAsDraft { get; set; } = true;
}
