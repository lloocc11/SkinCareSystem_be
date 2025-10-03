using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Aianalysis
{
    public Guid AnalysisId { get; set; }

    public Guid UserId { get; set; }

    public Guid ChatMessageId { get; set; }

    public string RawInput { get; set; } = null!;

    public string Result { get; set; } = null!;

    public double Confidence { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ChatMessage ChatMessage { get; set; } = null!;

    public virtual ICollection<Routine> Routines { get; set; } = new List<Routine>();

    public virtual User User { get; set; } = null!;
}
