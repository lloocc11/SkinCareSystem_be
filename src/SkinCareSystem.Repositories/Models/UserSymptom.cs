using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class UserSymptom
{
    public Guid user_symptom_id { get; set; }

    public Guid user_id { get; set; }

    public Guid symptom_id { get; set; }

    public DateTime? reported_at { get; set; }

    public virtual Symptom symptom { get; set; } = null!;

    public virtual User user { get; set; } = null!;
}
