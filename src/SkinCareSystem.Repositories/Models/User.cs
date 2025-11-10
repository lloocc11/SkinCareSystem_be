using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class User
{
    public Guid user_id { get; set; }

    public string full_name { get; set; } = null!;

    public string email { get; set; } = null!;

    public string google_id { get; set; } = null!;

    public Guid role_id { get; set; }

    public string? skin_type { get; set; }

    public DateOnly? date_of_birth { get; set; }

    public string status { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<AIAnalysis> AIAnalyses { get; set; } = new List<AIAnalysis>();

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();

    public virtual ICollection<ConsentRecord> ConsentRecords { get; set; } = new List<ConsentRecord>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<RoutineInstance> RoutineInstances { get; set; } = new List<RoutineInstance>();

    public virtual ICollection<Routine> Routines { get; set; } = new List<Routine>();

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    public virtual ICollection<UserQuery> UserQueries { get; set; } = new List<UserQuery>();

    public virtual ICollection<UserSymptom> UserSymptoms { get; set; } = new List<UserSymptom>();

    public virtual Role role { get; set; } = null!;
}
