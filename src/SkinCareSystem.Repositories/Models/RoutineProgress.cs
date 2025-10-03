using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class RoutineProgress
{
    public Guid ProgressId { get; set; }

    public Guid InstanceId { get; set; }

    public Guid StepId { get; set; }

    public DateTime CompletedAt { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public virtual RoutineInstance Instance { get; set; } = null!;

    public virtual RoutineStep Step { get; set; } = null!;
}
