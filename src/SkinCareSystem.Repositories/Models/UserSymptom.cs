using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class UserSymptom
{
    public Guid UserSymptomId { get; set; }

    public Guid UserId { get; set; }

    public Guid SymptomId { get; set; }

    public DateTime? ReportedAt { get; set; }

    public virtual Symptom Symptom { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
