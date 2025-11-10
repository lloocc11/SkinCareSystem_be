using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class RoutineInstance
{
    public Guid instance_id { get; set; }

    public Guid routine_id { get; set; }

    public Guid user_id { get; set; }

    public DateOnly start_date { get; set; }

    public DateOnly? end_date { get; set; }

    public string status { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public virtual ICollection<RoutineProgress> RoutineProgresses { get; set; } = new List<RoutineProgress>();

    public virtual Routine routine { get; set; } = null!;

    public virtual User user { get; set; } = null!;
}
