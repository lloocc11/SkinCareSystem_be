using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class QueryMatch
{
    public Guid MatchId { get; set; }

    public Guid QueryId { get; set; }

    public Guid ChunkId { get; set; }

    public double SimilarityScore { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual DocumentChunk Chunk { get; set; } = null!;

    public virtual UserQuery Query { get; set; } = null!;
}
