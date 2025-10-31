using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class ChatMessage
{
    public Guid message_id { get; set; }

    public Guid session_id { get; set; }

    public Guid? user_id { get; set; }

    public string? content { get; set; }

    public string? image_url { get; set; }

    public string message_type { get; set; } = null!;

    public string role { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public virtual ICollection<AIAnalysis> AIAnalyses { get; set; } = new List<AIAnalysis>();

    public virtual ChatSession session { get; set; } = null!;

    public virtual User? user { get; set; }
}
