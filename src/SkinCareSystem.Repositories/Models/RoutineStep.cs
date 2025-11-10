using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Các bước cụ thể trong liệu trình.
/// </summary>
public partial class RoutineStep
{
    /// <summary>
    /// Khóa chính bước liệu trình.
    /// </summary>
    public Guid step_id { get; set; }

    /// <summary>
    /// Liệu trình chứa bước này.
    /// </summary>
    public Guid routine_id { get; set; }

    /// <summary>
    /// Thứ tự thực hiện bước.
    /// </summary>
    public int step_order { get; set; }

    /// <summary>
    /// Hướng dẫn chi tiết.
    /// </summary>
    public string instruction { get; set; } = null!;

    /// <summary>
    /// Thời điểm trong ngày.
    /// </summary>
    public string time_of_day { get; set; } = null!;

    /// <summary>
    /// Tần suất thực hiện.
    /// </summary>
    public string frequency { get; set; } = null!;

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<RoutineProgress> RoutineProgresses { get; set; } = new List<RoutineProgress>();

    public virtual Routine routine { get; set; } = null!;
}
