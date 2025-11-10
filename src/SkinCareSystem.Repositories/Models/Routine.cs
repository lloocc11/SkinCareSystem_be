using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Routine
{
    public Guid routine_id { get; set; }

    public Guid user_id { get; set; }

    public Guid? analysis_id { get; set; }

    public string? description { get; set; }

    public int? version { get; set; }

    public Guid? parent_routine_id { get; set; }

    public string status { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Routine> Inverseparent_routine { get; set; } = new List<Routine>();

    public virtual ICollection<RoutineInstance> RoutineInstances { get; set; } = new List<RoutineInstance>();

    public virtual ICollection<RoutineStep> RoutineSteps { get; set; } = new List<RoutineStep>();

    public virtual AIAnalysis? analysis { get; set; }

    public virtual Routine? parent_routine { get; set; }

    public virtual User user { get; set; } = null!;
}
