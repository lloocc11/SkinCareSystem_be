using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class RoutineStep
{
    public Guid StepId { get; set; }

    public Guid RoutineId { get; set; }

    public int StepOrder { get; set; }

    public string Instruction { get; set; } = null!;

    public string TimeOfDay { get; set; } = null!;

    public string Frequency { get; set; } = null!;

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual Routine Routine { get; set; } = null!;

    public virtual ICollection<RoutineProgress> RoutineProgresses { get; set; } = new List<RoutineProgress>();
}
