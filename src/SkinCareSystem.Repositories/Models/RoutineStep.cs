using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class RoutineStep
{
    public Guid step_id { get; set; }

    public Guid routine_id { get; set; }

    public int step_order { get; set; }

    public string instruction { get; set; } = null!;

    public string time_of_day { get; set; } = null!;

    public string frequency { get; set; } = null!;

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<RoutineProgress> RoutineProgresses { get; set; } = new List<RoutineProgress>();

    public virtual Routine routine { get; set; } = null!;
}
