using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Routine
{
    public Guid RoutineId { get; set; }

    public Guid UserId { get; set; }

    public Guid? AnalysisId { get; set; }

    public string? Description { get; set; }

    public int? Version { get; set; }

    public Guid? ParentRoutineId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Aianalysis? Analysis { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Routine> InverseParentRoutine { get; set; } = new List<Routine>();

    public virtual Routine? ParentRoutine { get; set; }

    public virtual ICollection<RoutineInstance> RoutineInstances { get; set; } = new List<RoutineInstance>();

    public virtual ICollection<RoutineStep> RoutineSteps { get; set; } = new List<RoutineStep>();

    public virtual User User { get; set; } = null!;
}
