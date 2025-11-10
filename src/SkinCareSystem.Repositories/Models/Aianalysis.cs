using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class AIAnalysis
{
    public Guid analysis_id { get; set; }

    public Guid user_id { get; set; }

    public Guid chat_message_id { get; set; }

    public string raw_input { get; set; } = null!;

    public string result { get; set; } = null!;

    public double confidence { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<Routine> Routines { get; set; } = new List<Routine>();

    public virtual ChatMessage chat_message { get; set; } = null!;

    public virtual User user { get; set; } = null!;
}
