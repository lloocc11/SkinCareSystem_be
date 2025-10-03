using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Symptom
{
    public Guid SymptomId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ExampleImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<RuleCondition> RuleConditions { get; set; } = new List<RuleCondition>();

    public virtual ICollection<UserSymptom> UserSymptoms { get; set; } = new List<UserSymptom>();
}
