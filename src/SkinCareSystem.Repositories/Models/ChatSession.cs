using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class ChatSession
{
    public Guid session_id { get; set; }

    public Guid user_id { get; set; }

    public string? title { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual User user { get; set; } = null!;
}
