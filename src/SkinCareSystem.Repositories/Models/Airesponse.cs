using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Phản hồi AI sau truy vấn người dùng.
/// </summary>
public partial class AIResponse
{
    /// <summary>
    /// Khóa chính phản hồi.
    /// </summary>
    public Guid response_id { get; set; }

    /// <summary>
    /// Truy vấn nguồn.
    /// </summary>
    public Guid query_id { get; set; }

    /// <summary>
    /// Nội dung phản hồi.
    /// </summary>
    public string response_text { get; set; } = null!;

    /// <summary>
    /// Loại phản hồi (recommendation/explanation/disclaimer).
    /// </summary>
    public string response_type { get; set; } = null!;

    /// <summary>
    /// Thời điểm tạo phản hồi.
    /// </summary>
    public DateTime? created_at { get; set; }

    public virtual UserQuery query { get; set; } = null!;
}
