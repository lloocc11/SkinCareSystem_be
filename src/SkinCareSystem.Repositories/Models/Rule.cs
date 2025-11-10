using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Tập luật gợi ý/skincare.
/// </summary>
public partial class Rule
{
    /// <summary>
    /// Khóa chính luật.
    /// </summary>
    public Guid rule_id { get; set; }

    /// <summary>
    /// Khuyến nghị tương ứng.
    /// </summary>
    public string recommendation { get; set; } = null!;

    /// <summary>
    /// Mức độ khẩn cấp của luật.
    /// </summary>
    public string urgency_level { get; set; } = null!;

    /// <summary>
    /// Thời điểm tạo luật.
    /// </summary>
    public DateTime? created_at { get; set; }

    public virtual ICollection<RuleCondition> RuleConditions { get; set; } = new List<RuleCondition>();
}
