using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Bản ghi đồng ý của người dùng.
/// </summary>
public partial class ConsentRecord
{
    /// <summary>
    /// Khóa chính đồng ý.
    /// </summary>
    public Guid consent_id { get; set; }

    /// <summary>
    /// Người dùng cho phép.
    /// </summary>
    public Guid user_id { get; set; }

    /// <summary>
    /// Loại đồng ý.
    /// </summary>
    public string consent_type { get; set; } = null!;

    /// <summary>
    /// Nội dung đồng ý.
    /// </summary>
    public string consent_text { get; set; } = null!;

    /// <summary>
    /// Trạng thái đồng ý.
    /// </summary>
    public bool given { get; set; }

    /// <summary>
    /// Thời điểm xác nhận.
    /// </summary>
    public DateTime? given_at { get; set; }

    public virtual User user { get; set; } = null!;
}
