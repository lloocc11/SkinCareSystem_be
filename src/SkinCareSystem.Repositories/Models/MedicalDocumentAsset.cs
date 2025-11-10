using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Tập hợp ảnh/tệp đính kèm cho tài liệu y khoa.
/// </summary>
public partial class MedicalDocumentAsset
{
    /// <summary>
    /// Khóa chính bản ghi tài nguyên.
    /// </summary>
    public Guid asset_id { get; set; }

    /// <summary>
    /// Tài liệu y khoa sở hữu ảnh.
    /// </summary>
    public Guid doc_id { get; set; }

    /// <summary>
    /// URL công khai của ảnh/tệp.
    /// </summary>
    public string file_url { get; set; } = null!;

    /// <summary>
    /// Public ID từ Cloudinary (hoặc provider).
    /// </summary>
    public string? public_id { get; set; }

    /// <summary>
    /// Tên nhà cung cấp lưu trữ (mặc định Cloudinary).
    /// </summary>
    public string? provider { get; set; }

    /// <summary>
    /// Loại MIME của tài nguyên.
    /// </summary>
    public string? mime_type { get; set; }

    /// <summary>
    /// Dung lượng tệp (byte).
    /// </summary>
    public int? size_bytes { get; set; }

    /// <summary>
    /// Chiều rộng ảnh (pixel).
    /// </summary>
    public int? width { get; set; }

    /// <summary>
    /// Chiều cao ảnh (pixel).
    /// </summary>
    public int? height { get; set; }

    /// <summary>
    /// Thời điểm lưu tài nguyên.
    /// </summary>
    public DateTime? created_at { get; set; }

    public virtual MedicalDocument doc { get; set; } = null!;
}
