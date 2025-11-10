using System;
using System.Collections.Generic;
using Pgvector;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Các đoạn văn bản đã chia nhỏ từ tài liệu.
/// </summary>
public partial class DocumentChunk
{
    /// <summary>
    /// Khóa chính đoạn văn.
    /// </summary>
    public Guid chunk_id { get; set; }

    /// <summary>
    /// Tài liệu gốc chứa đoạn văn.
    /// </summary>
    public Guid doc_id { get; set; }

    /// <summary>
    /// Nội dung đoạn.
    /// </summary>
    public string chunk_text { get; set; } = null!;

    /// <summary>
    /// Vector(1536) dùng cho tìm kiếm ngữ nghĩa.
    /// </summary>
    public Vector? embedding { get; set; }

    /// <summary>
    /// Thời điểm tạo đoạn.
    /// </summary>
    public DateTime? created_at { get; set; }

    public virtual ICollection<QueryMatch> QueryMatches { get; set; } = new List<QueryMatch>();

    public virtual MedicalDocument doc { get; set; } = null!;
}
