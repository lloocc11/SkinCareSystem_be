using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Liệu trình chăm sóc da do AI đề xuất.
/// </summary>
public partial class Routine
{
    /// <summary>
    /// Khóa chính liệu trình.
    /// </summary>
    public Guid routine_id { get; set; }

    /// <summary>
    /// Người dùng sở hữu liệu trình.
    /// </summary>
    public Guid user_id { get; set; }

    /// <summary>
    /// Phân tích AI liên quan.
    /// </summary>
    public Guid? analysis_id { get; set; }

    /// <summary>
    /// Mô tả chung liệu trình.
    /// </summary>
    public string? description { get; set; }

    public string? target_skin_type { get; set; }

    public string? target_conditions { get; set; }

    public string routine_type { get; set; } = null!;

    /// <summary>
    /// Phiên bản liệu trình.
    /// </summary>
    public int? version { get; set; }

    /// <summary>
    /// Liệu trình cha (nếu có).
    /// </summary>
    public Guid? parent_routine_id { get; set; }

    /// <summary>
    /// Trạng thái liệu trình.
    /// </summary>
    public string status { get; set; } = null!;

    /// <summary>
    /// Thời điểm tạo liệu trình.
    /// </summary>
    public DateTime? created_at { get; set; }

    /// <summary>
    /// Thời điểm cập nhật liệu trình.
    /// </summary>
    public DateTime? updated_at { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Routine> Inverseparent_routine { get; set; } = new List<Routine>();

    public virtual ICollection<RoutineInstance> RoutineInstances { get; set; } = new List<RoutineInstance>();

    public virtual ICollection<RoutineStep> RoutineSteps { get; set; } = new List<RoutineStep>();

    public virtual AIAnalysis? analysis { get; set; }

    public virtual Routine? parent_routine { get; set; }

    public virtual User user { get; set; } = null!;
}
