using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class MedicalDocument
{
    public Guid DocId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? Source { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? LastUpdated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<DocumentChunk> DocumentChunks { get; set; } = new List<DocumentChunk>();
}
