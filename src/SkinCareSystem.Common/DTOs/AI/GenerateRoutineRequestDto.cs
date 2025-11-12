using System.Collections.Generic;

namespace SkinCareSystem.Common.DTOs.AI;

public class GenerateRoutineRequestDto
{
    public string Query { get; set; } = string.Empty;
    public string? TargetSkinType { get; set; }
    public IList<string> TargetConditions { get; set; } = new List<string>();

    /// <summary>
    /// Ngữ cảnh thô bổ sung (ví dụ trích xuất từ file upload) để đưa vào prompt.
    /// </summary>
    public string? AdditionalContext { get; set; }

    public int MaxSteps { get; set; } = 10;
    public int NumVariants { get; set; } = 1;
    public bool AutoSaveAsDraft { get; set; } = true;

    /// <summary>
    /// Danh sách URL ảnh (Cloudinary hoặc CDN) được dùng để mô tả tình trạng da.
    /// </summary>
    public IList<string> ImageUrls { get; set; } = new List<string>();
}
