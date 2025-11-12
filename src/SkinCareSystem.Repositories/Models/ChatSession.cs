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
    /// Thời điểm tạo phiên.
    /// </summary>
    public DateTime? created_at { get; set; }

    /// <summary>
    /// Thời điểm cập nhật phiên.
    /// </summary>
    public DateTime? updated_at { get; set; }

    /// <summary>
    /// ai: user↔AI (templates-only); specialist: user↔specialist; ai_admin: admin/specialist↔AI (builder, có thể tạo routine mới).
    /// </summary>
    public string channel { get; set; } = null!;

    /// <summary>
    /// User_id của specialist đang phụ trách phiên (NULL nếu chưa gán/không áp dụng).
    /// </summary>
    public Guid? specialist_id { get; set; }

    /// <summary>
    /// Thời điểm assign specialist (áp dụng cho channel=specialist).
    /// </summary>
    public DateTime? assigned_at { get; set; }

    /// <summary>
    /// Thời điểm đóng phiên (tuỳ workflow).
    /// </summary>
    public DateTime? closed_at { get; set; }

    /// <summary>
    /// Trạng thái phiên chat:
    /// - open: Phiên đang mở (AI hoặc admin-special use-case), chưa cần specialist.
    /// - waiting_specialist: Phiên kênh specialist chưa có ai nhận (specialist_id IS NULL).
    /// - assigned: Phiên kênh specialist đã được gán cho 1 specialist (specialist_id NOT NULL), chưa đóng.
    /// - closed: Phiên đã đóng (closed_at NOT NULL).
    /// </summary>
    public string state { get; set; } = null!;

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual User? specialist { get; set; }

    public virtual User user { get; set; } = null!;
}
