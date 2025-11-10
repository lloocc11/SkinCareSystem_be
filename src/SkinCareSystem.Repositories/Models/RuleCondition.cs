using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Điều kiện áp dụng cho mỗi luật.
/// </summary>
public partial class RuleCondition
{
    /// <summary>
    /// Khóa chính điều kiện.
    /// </summary>
    public Guid rule_condition_id { get; set; }

    /// <summary>
    /// Luật được áp dụng.
    /// </summary>
    public Guid rule_id { get; set; }

    /// <summary>
    /// Triệu chứng điều kiện.
    /// </summary>
    public Guid? symptom_id { get; set; }

    /// <summary>
    /// Câu hỏi điều kiện.
    /// </summary>
    public Guid? question_id { get; set; }

    /// <summary>
    /// Toán tử so sánh.
    /// </summary>
    public string _operator { get; set; } = null!;

    /// <summary>
    /// Giá trị so sánh.
    /// </summary>
    public string? value { get; set; }

    public virtual Question? question { get; set; }

    public virtual Rule rule { get; set; } = null!;

    public virtual Symptom? symptom { get; set; }
}
