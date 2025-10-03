using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Question
{
    public Guid QuestionId { get; set; }

    public string Text { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string? Options { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<RuleCondition> RuleConditions { get; set; } = new List<RuleCondition>();

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
