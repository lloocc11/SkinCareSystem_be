using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class AIResponse
{
    public Guid response_id { get; set; }

    public Guid query_id { get; set; }

    public string response_text { get; set; } = null!;

    public string response_type { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public virtual UserQuery query { get; set; } = null!;
}
