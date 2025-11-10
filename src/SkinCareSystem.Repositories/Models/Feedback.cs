using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Phản hồi của người dùng về liệu trình/bước.
/// </summary>
public partial class Feedback
{
    /// <summary>
    /// Khóa chính phản hồi.
    /// </summary>
    public Guid feedback_id { get; set; }

    /// <summary>
    /// Liệu trình được phản hồi.
    /// </summary>
    public Guid routine_id { get; set; }

    /// <summary>
    /// Bước liên quan (nếu có).
    /// </summary>
    public Guid? step_id { get; set; }

    /// <summary>
    /// Người dùng phản hồi.
    /// </summary>
    public Guid user_id { get; set; }

    /// <summary>
    /// Điểm đánh giá (1-5).
    /// </summary>
    public int rating { get; set; }

    /// <summary>
    /// Nội dung phản hồi.
    /// </summary>
    public string? comment { get; set; }

    /// <summary>
    /// Thời điểm tạo phản hồi.
    /// </summary>
    public DateTime? created_at { get; set; }

    public virtual Routine routine { get; set; } = null!;

    public virtual RoutineStep? step { get; set; }

    public virtual User user { get; set; } = null!;
}
