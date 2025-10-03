using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Feedback
{
    public Guid FeedbackId { get; set; }

    public Guid RoutineId { get; set; }

    public Guid? StepId { get; set; }

    public Guid UserId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Routine Routine { get; set; } = null!;

    public virtual RoutineStep? Step { get; set; }

    public virtual User User { get; set; } = null!;
}
