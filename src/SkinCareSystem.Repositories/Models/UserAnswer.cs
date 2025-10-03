using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class UserAnswer
{
    public Guid AnswerId { get; set; }

    public Guid UserId { get; set; }

    public Guid QuestionId { get; set; }

    public string AnswerValue { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
