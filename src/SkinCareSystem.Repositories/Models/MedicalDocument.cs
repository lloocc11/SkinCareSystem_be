using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class MedicalDocument
{
    public Guid doc_id { get; set; }

    public string title { get; set; } = null!;

    public string content { get; set; } = null!;

    public string? source { get; set; }

    public string status { get; set; } = null!;

    public DateTime? last_updated { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<DocumentChunk> DocumentChunks { get; set; } = new List<DocumentChunk>();

    public virtual ICollection<MedicalDocumentAsset> MedicalDocumentAssets { get; set; } = new List<MedicalDocumentAsset>();
}
