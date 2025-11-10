using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Phiên trò chuyện giữa người dùng và AI.
/// </summary>
public partial class ChatSession
{
    /// <summary>
    /// Khóa chính phiên chat.
    /// </summary>
    public Guid session_id { get; set; }

    /// <summary>
    /// Chủ sở hữu phiên chat.
    /// </summary>
    public Guid user_id { get; set; }

    /// <summary>
    /// Tiêu đề phiên chat.
    /// </summary>
    public string? title { get; set; }

    /// <summary>
    /// Trạng thái phiên chat.
    /// </summary>
    public string? status { get; set; }

    /// <summary>
    /// Thời điểm tạo phiên.
    /// </summary>
    public DateTime? created_at { get; set; }

    /// <summary>
    /// Thời điểm cập nhật phiên.
    /// </summary>
    public DateTime? updated_at { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual User user { get; set; } = null!;
}
