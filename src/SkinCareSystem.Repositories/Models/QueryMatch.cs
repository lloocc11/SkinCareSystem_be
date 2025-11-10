using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class QueryMatch
{
    public Guid match_id { get; set; }

    public Guid query_id { get; set; }

    public Guid chunk_id { get; set; }

    public double similarity_score { get; set; }

    public DateTime? created_at { get; set; }

    public virtual DocumentChunk chunk { get; set; } = null!;

    public virtual UserQuery query { get; set; } = null!;
}
