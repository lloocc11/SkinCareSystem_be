using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Tiến độ thực hiện từng bước.
/// </summary>
public partial class RoutineProgress
{
    /// <summary>
    /// Khóa chính tiến độ.
    /// </summary>
    public Guid progress_id { get; set; }

    /// <summary>
    /// Instance liên quan.
    /// </summary>
    public Guid instance_id { get; set; }

    /// <summary>
    /// Bước được ghi nhận.
    /// </summary>
    public Guid step_id { get; set; }

    /// <summary>
    /// Thời điểm hoàn tất.
    /// </summary>
    public DateTime completed_at { get; set; }

    /// <summary>
    /// Ảnh minh chứng (nếu có).
    /// </summary>
    public string? photo_url { get; set; }

    /// <summary>
    /// Ghi chú thêm.
    /// </summary>
    public string? note { get; set; }

    /// <summary>
    /// Trạng thái tiến độ.
    /// </summary>
    public string status { get; set; } = null!;

    public int? irritation_level { get; set; }

    public string? mood_note { get; set; }

    public virtual RoutineInstance instance { get; set; } = null!;

    public virtual RoutineStep step { get; set; } = null!;
}
