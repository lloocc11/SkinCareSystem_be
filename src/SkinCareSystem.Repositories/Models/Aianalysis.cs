using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Kết quả phân tích AI cho từng tin nhắn.
/// </summary>
public partial class AIAnalysis
{
    /// <summary>
    /// Khóa chính bản ghi phân tích.
    /// </summary>
    public Guid analysis_id { get; set; }

    /// <summary>
    /// Người dùng được phân tích.
    /// </summary>
    public Guid user_id { get; set; }

    /// <summary>
    /// Tin nhắn nguồn được phân tích.
    /// </summary>
    public Guid chat_message_id { get; set; }

    /// <summary>
    /// Dữ liệu đầu vào gốc.
    /// </summary>
    public string raw_input { get; set; } = null!;

    /// <summary>
    /// Kết quả phân tích dạng JSON.
    /// </summary>
    public string result { get; set; } = null!;

    /// <summary>
    /// Độ tin cậy của phân tích (0-1).
    /// </summary>
    public double confidence { get; set; }

    /// <summary>
    /// Thời điểm tạo phân tích.
    /// </summary>
    public DateTime? created_at { get; set; }

    /// <summary>
    /// Thời điểm cập nhật phân tích.
    /// </summary>
    public DateTime? updated_at { get; set; }

    public virtual ICollection<Routine> Routines { get; set; } = new List<Routine>();

    public virtual ChatMessage chat_message { get; set; } = null!;

    public virtual User user { get; set; } = null!;
}
