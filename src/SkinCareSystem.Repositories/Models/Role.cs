using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Danh mục vai trò người dùng.
/// </summary>
public partial class Role
{
    /// <summary>
    /// Khóa chính vai trò.
    /// </summary>
    public Guid role_id { get; set; }

    /// <summary>
    /// Tên vai trò duy nhất.
    /// </summary>
    public string name { get; set; } = null!;

    /// <summary>
    /// Mô tả vai trò.
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của vai trò.
    /// </summary>
    public string status { get; set; } = null!;

    /// <summary>
    /// Thời điểm tạo vai trò.
    /// </summary>
    public DateTime? created_at { get; set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất.
    /// </summary>
    public DateTime? updated_at { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
