using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Rule
{
    public Guid RuleId { get; set; }

    public string Recommendation { get; set; } = null!;

    public string UrgencyLevel { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<RuleCondition> RuleConditions { get; set; } = new List<RuleCondition>();
}
