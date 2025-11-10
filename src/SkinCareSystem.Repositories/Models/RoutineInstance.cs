using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Lần triển khai liệu trình theo thời gian.
/// </summary>
public partial class RoutineInstance
{
    /// <summary>
    /// Khóa chính instance.
    /// </summary>
    public Guid instance_id { get; set; }

    /// <summary>
    /// Liệu trình triển khai.
    /// </summary>
    public Guid routine_id { get; set; }

    /// <summary>
    /// Người dùng áp dụng.
    /// </summary>
    public Guid user_id { get; set; }

    /// <summary>
    /// Ngày bắt đầu.
    /// </summary>
    public DateOnly start_date { get; set; }

    /// <summary>
    /// Ngày kết thúc (nếu có).
    /// </summary>
    public DateOnly? end_date { get; set; }

    /// <summary>
    /// Trạng thái thực hiện.
    /// </summary>
    public string status { get; set; } = null!;

    public decimal? adherence_score { get; set; }

    /// <summary>
    /// Thời điểm tạo instance.
    /// </summary>
    public DateTime? created_at { get; set; }

    public virtual ICollection<RoutineProgress> RoutineProgresses { get; set; } = new List<RoutineProgress>();

    public virtual Routine routine { get; set; } = null!;

    public virtual User user { get; set; } = null!;
}
