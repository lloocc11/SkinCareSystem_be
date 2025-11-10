using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Danh sách triệu chứng da liễu.
/// </summary>
public partial class Symptom
{
    /// <summary>
    /// Khóa chính triệu chứng.
    /// </summary>
    public Guid symptom_id { get; set; }

    /// <summary>
    /// Tên triệu chứng duy nhất.
    /// </summary>
    public string name { get; set; } = null!;

    /// <summary>
    /// Mô tả chi tiết.
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// Ảnh minh họa.
    /// </summary>
    public string? example_image_url { get; set; }

    /// <summary>
    /// Thời điểm tạo triệu chứng.
    /// </summary>
    public DateTime? created_at { get; set; }

    public virtual ICollection<RuleCondition> RuleConditions { get; set; } = new List<RuleCondition>();

    public virtual ICollection<UserSymptom> UserSymptoms { get; set; } = new List<UserSymptom>();
}
