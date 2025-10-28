using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class RoutineProgress
{
    public Guid progress_id { get; set; }

    public Guid instance_id { get; set; }

    public Guid step_id { get; set; }

    public DateTime completed_at { get; set; }

    public string? photo_url { get; set; }

    public string? note { get; set; }

    public string status { get; set; } = null!;

    public virtual RoutineInstance instance { get; set; } = null!;

    public virtual RoutineStep step { get; set; } = null!;
}
