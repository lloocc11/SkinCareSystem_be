using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class RuleCondition
{
    public Guid rule_condition_id { get; set; }

    public Guid rule_id { get; set; }

    public Guid? symptom_id { get; set; }

    public Guid? question_id { get; set; }

    public string _operator { get; set; } = null!;

    public string? value { get; set; }

    public virtual Question? question { get; set; }

    public virtual Rule rule { get; set; } = null!;

    public virtual Symptom? symptom { get; set; }
}
