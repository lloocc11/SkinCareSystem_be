using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Tài liệu tri thức y khoa phục vụ RAG.
/// </summary>
public partial class MedicalDocument
{
    /// <summary>
    /// Khóa chính tài liệu.
    /// </summary>
    public Guid doc_id { get; set; }

    /// <summary>
    /// Tiêu đề tài liệu.
    /// </summary>
    public string title { get; set; } = null!;

    /// <summary>
    /// Nội dung tài liệu.
    /// </summary>
    public string content { get; set; } = null!;

    /// <summary>
    /// Nguồn trích dẫn tài liệu.
    /// </summary>
    public string? source { get; set; }

    /// <summary>
    /// Trạng thái hiển thị tài liệu.
    /// </summary>
    public string status { get; set; } = null!;

    /// <summary>
    /// Thời điểm cập nhật nội dung gần nhất.
    /// </summary>
    public DateTime? last_updated { get; set; }

    /// <summary>
    /// Thời điểm tạo tài liệu.
    /// </summary>
    public DateTime? created_at { get; set; }

    public virtual ICollection<DocumentChunk> DocumentChunks { get; set; } = new List<DocumentChunk>();

    public virtual ICollection<MedicalDocumentAsset> MedicalDocumentAssets { get; set; } = new List<MedicalDocumentAsset>();
}
