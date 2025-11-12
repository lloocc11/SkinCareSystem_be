using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

/// <summary>
/// Thông tin người dùng hệ thống.
/// </summary>
public partial class User
{
    /// <summary>
    /// Khóa chính người dùng.
    /// </summary>
    public Guid user_id { get; set; }

    /// <summary>
    /// Họ và tên đầy đủ.
    /// </summary>
    public string full_name { get; set; } = null!;

    /// <summary>
    /// Email đăng nhập duy nhất.
    /// </summary>
    public string email { get; set; } = null!;

    /// <summary>
    /// ID Google dùng để xác thực.
    /// </summary>
    public string? google_id { get; set; }

    public string? password_hash { get; set; }

    public string auth_provider { get; set; } = null!;

    /// <summary>
    /// Vai trò của người dùng.
    /// </summary>
    public Guid role_id { get; set; }

    /// <summary>
    /// Loại da của người dùng.
    /// </summary>
    public string? skin_type { get; set; }

    /// <summary>
    /// Ngày sinh của người dùng.
    /// </summary>
    public DateOnly? date_of_birth { get; set; }

    /// <summary>
    /// Trạng thái người dùng (active/inactive/banned).
    /// </summary>
    public string status { get; set; } = null!;

    /// <summary>
    /// Thời điểm tạo người dùng.
    /// </summary>
    public DateTime? created_at { get; set; }

    /// <summary>
    /// Thời điểm cập nhật người dùng.
    /// </summary>
    public DateTime? updated_at { get; set; }

    public virtual ICollection<AIAnalysis> AIAnalyses { get; set; } = new List<AIAnalysis>();

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatSession> ChatSessionspecialists { get; set; } = new List<ChatSession>();

    public virtual ICollection<ChatSession> ChatSessionusers { get; set; } = new List<ChatSession>();

    public virtual ICollection<ConsentRecord> ConsentRecords { get; set; } = new List<ConsentRecord>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<RoutineInstance> RoutineInstances { get; set; } = new List<RoutineInstance>();

    public virtual ICollection<Routine> Routines { get; set; } = new List<Routine>();

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    public virtual ICollection<UserQuery> UserQueries { get; set; } = new List<UserQuery>();

    public virtual ICollection<UserSymptom> UserSymptoms { get; set; } = new List<UserSymptom>();

    public virtual Role role { get; set; } = null!;
}
