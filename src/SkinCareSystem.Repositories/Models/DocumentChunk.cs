using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class DocumentChunk
{
    public Guid ChunkId { get; set; }

    public Guid DocId { get; set; }

    public string ChunkText { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual MedicalDocument Doc { get; set; } = null!;

    public virtual ICollection<QueryMatch> QueryMatches { get; set; } = new List<QueryMatch>();
}
