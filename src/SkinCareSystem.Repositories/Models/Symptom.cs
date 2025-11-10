using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Symptom
{
    public Guid symptom_id { get; set; }

    public string name { get; set; } = null!;

    public string? description { get; set; }

    public string? example_image_url { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<RuleCondition> RuleConditions { get; set; } = new List<RuleCondition>();

    public virtual ICollection<UserSymptom> UserSymptoms { get; set; } = new List<UserSymptom>();
}
