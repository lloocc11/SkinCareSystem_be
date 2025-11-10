using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class UserAnswer
{
    public Guid answer_id { get; set; }

    public Guid user_id { get; set; }

    public Guid question_id { get; set; }

    public string answer_value { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual Question question { get; set; } = null!;

    public virtual User user { get; set; } = null!;
}
