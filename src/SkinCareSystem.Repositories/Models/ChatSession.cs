using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class ChatSession
{
    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public string? Title { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual User User { get; set; } = null!;
}
