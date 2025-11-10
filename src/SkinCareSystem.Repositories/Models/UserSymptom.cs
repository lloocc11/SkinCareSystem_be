using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Ghi nhận triệu chứng người dùng gặp phải.
/// </summary>
public partial class UserSymptom
{
    /// <summary>
    /// Khóa chính bản ghi triệu chứng.
    /// </summary>
    public Guid user_symptom_id { get; set; }

    /// <summary>
    /// Người dùng liên quan.
    /// </summary>
    public Guid user_id { get; set; }

    /// <summary>
    /// Triệu chứng đã chọn.
    /// </summary>
    public Guid symptom_id { get; set; }

    /// <summary>
    /// Thời điểm báo cáo triệu chứng.
    /// </summary>
    public DateTime? reported_at { get; set; }

    public virtual Symptom symptom { get; set; } = null!;

    public virtual User user { get; set; } = null!;
}
