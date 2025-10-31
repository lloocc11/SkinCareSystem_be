using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class UserQuery
{
    public Guid query_id { get; set; }

    public Guid user_id { get; set; }

    public string query_text { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public virtual ICollection<AIResponse> AIResponses { get; set; } = new List<AIResponse>();

    public virtual ICollection<QueryMatch> QueryMatches { get; set; } = new List<QueryMatch>();

    public virtual User user { get; set; } = null!;
}
