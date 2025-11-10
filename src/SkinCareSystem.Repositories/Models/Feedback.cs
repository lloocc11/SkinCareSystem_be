using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Feedback
{
    public Guid feedback_id { get; set; }

    public Guid routine_id { get; set; }

    public Guid? step_id { get; set; }

    public Guid user_id { get; set; }

    public int rating { get; set; }

    public string? comment { get; set; }

    public DateTime? created_at { get; set; }

    public virtual Routine routine { get; set; } = null!;

    public virtual RoutineStep? step { get; set; }

    public virtual User user { get; set; } = null!;
}
