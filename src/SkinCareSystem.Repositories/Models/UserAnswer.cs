using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Câu trả lời khảo sát của người dùng.
/// </summary>
public partial class UserAnswer
{
    /// <summary>
    /// Khóa chính câu trả lời.
    /// </summary>
    public Guid answer_id { get; set; }

    /// <summary>
    /// Người dùng thực hiện trả lời.
    /// </summary>
    public Guid user_id { get; set; }

    /// <summary>
    /// Câu hỏi được trả lời.
    /// </summary>
    public Guid question_id { get; set; }

    /// <summary>
    /// Giá trị câu trả lời.
    /// </summary>
    public string answer_value { get; set; } = null!;

    /// <summary>
    /// Thời điểm tạo câu trả lời.
    /// </summary>
    public DateTime? created_at { get; set; }

    /// <summary>
    /// Thời điểm cập nhật câu trả lời.
    /// </summary>
    public DateTime? updated_at { get; set; }

    public virtual Question question { get; set; } = null!;

    public virtual User user { get; set; } = null!;
}
