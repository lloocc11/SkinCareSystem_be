using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Kết quả đối sánh đoạn văn cho truy vấn.
/// </summary>
public partial class QueryMatch
{
    /// <summary>
    /// Khóa chính bản ghi đối sánh.
    /// </summary>
    public Guid match_id { get; set; }

    /// <summary>
    /// Truy vấn liên quan.
    /// </summary>
    public Guid query_id { get; set; }

    /// <summary>
    /// Đoạn văn được đối sánh.
    /// </summary>
    public Guid chunk_id { get; set; }

    /// <summary>
    /// Điểm tương đồng (0-1).
    /// </summary>
    public double similarity_score { get; set; }

    /// <summary>
    /// Thời điểm tạo bản ghi.
    /// </summary>
    public DateTime? created_at { get; set; }

    public virtual DocumentChunk chunk { get; set; } = null!;

    public virtual UserQuery query { get; set; } = null!;
}
