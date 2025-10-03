using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class RuleCondition
{
    public Guid RuleConditionId { get; set; }

    public Guid RuleId { get; set; }

    public Guid? SymptomId { get; set; }

    public Guid? QuestionId { get; set; }

    public string Operator { get; set; } = null!;

    public string? Value { get; set; }

    public virtual Question? Question { get; set; }

    public virtual Rule Rule { get; set; } = null!;

    public virtual Symptom? Symptom { get; set; }
}
