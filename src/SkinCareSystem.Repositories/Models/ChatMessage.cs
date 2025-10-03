using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class ChatMessage
{
    public Guid MessageId { get; set; }

    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public string? Content { get; set; }

    public string? ImageUrl { get; set; }

    public string MessageType { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Aianalysis> Aianalyses { get; set; } = new List<Aianalysis>();

    public virtual ChatSession Session { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
