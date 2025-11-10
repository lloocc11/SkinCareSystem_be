using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Tin nhắn trong phiên trò chuyện.
/// </summary>
public partial class ChatMessage
{
    /// <summary>
    /// Khóa chính tin nhắn.
    /// </summary>
    public Guid message_id { get; set; }

    /// <summary>
    /// Phiên chat chứa tin nhắn.
    /// </summary>
    public Guid session_id { get; set; }

    /// <summary>
    /// Người gửi (user hoặc bot).
    /// </summary>
    public Guid? user_id { get; set; }

    /// <summary>
    /// Nội dung văn bản.
    /// </summary>
    public string? content { get; set; }

    /// <summary>
    /// URL ảnh đính kèm (nếu có).
    /// </summary>
    public string? image_url { get; set; }

    /// <summary>
    /// Loại tin nhắn (text/image/mixed).
    /// </summary>
    public string message_type { get; set; } = null!;

    /// <summary>
    /// Thời điểm gửi tin nhắn.
    /// </summary>
    public DateTime? created_at { get; set; }

    public virtual ICollection<AIAnalysis> AIAnalyses { get; set; } = new List<AIAnalysis>();

    public virtual ChatSession session { get; set; } = null!;

    public virtual User? user { get; set; }
}
