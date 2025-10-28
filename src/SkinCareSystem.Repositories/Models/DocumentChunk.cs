using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class DocumentChunk
{
    public Guid chunk_id { get; set; }

    public Guid doc_id { get; set; }

    public string chunk_text { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public virtual ICollection<QueryMatch> QueryMatches { get; set; } = new List<QueryMatch>();

    public virtual MedicalDocument doc { get; set; } = null!;
}
