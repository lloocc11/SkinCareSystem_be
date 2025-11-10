using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Question
{
    public Guid question_id { get; set; }

    public string text { get; set; } = null!;

    public string type { get; set; } = null!;

    public string? options { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<RuleCondition> RuleConditions { get; set; } = new List<RuleCondition>();

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
