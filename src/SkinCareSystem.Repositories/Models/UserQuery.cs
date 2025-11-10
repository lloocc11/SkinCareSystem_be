using System;
using System.Collections.Generic;
using Pgvector;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Lịch sử truy vấn của người dùng.
/// </summary>
public partial class UserQuery
{
    /// <summary>
    /// Khóa chính truy vấn.
    /// </summary>
    public Guid query_id { get; set; }

    /// <summary>
    /// Người dùng thực hiện truy vấn.
    /// </summary>
    public Guid user_id { get; set; }

    /// <summary>
    /// Nội dung câu hỏi.
    /// </summary>
    public string query_text { get; set; } = null!;

    /// <summary>
    /// Vector(1536) biểu diễn truy vấn.
    /// </summary>
    public Vector? query_embedding { get; set; }

    /// <summary>
    /// Thời điểm tạo truy vấn.
    /// </summary>
    public DateTime? created_at { get; set; }

    public virtual ICollection<AIResponse> AIResponses { get; set; } = new List<AIResponse>();

    public virtual ICollection<QueryMatch> QueryMatches { get; set; } = new List<QueryMatch>();

    public virtual User user { get; set; } = null!;
}
