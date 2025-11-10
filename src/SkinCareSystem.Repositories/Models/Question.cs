using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Các câu hỏi khảo sát người dùng.
/// </summary>
public partial class Question
{
    /// <summary>
    /// Khóa chính câu hỏi.
    /// </summary>
    public Guid question_id { get; set; }

    /// <summary>
    /// Nội dung câu hỏi.
    /// </summary>
    public string text { get; set; } = null!;

    /// <summary>
    /// Loại câu hỏi (choice/multi-choice/text).
    /// </summary>
    public string type { get; set; } = null!;

    /// <summary>
    /// Danh sách lựa chọn (JSON).
    /// </summary>
    public string? options { get; set; }

    /// <summary>
    /// Thời điểm tạo câu hỏi.
    /// </summary>
    public DateTime? created_at { get; set; }

    public virtual ICollection<RuleCondition> RuleConditions { get; set; } = new List<RuleCondition>();

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
