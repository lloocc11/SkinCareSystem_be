using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Rule
{
    public Guid rule_id { get; set; }

    public string recommendation { get; set; } = null!;

    public string urgency_level { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public virtual ICollection<RuleCondition> RuleConditions { get; set; } = new List<RuleCondition>();
}
