using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class MedicalDocumentAsset
{
    public Guid asset_id { get; set; }

    public Guid doc_id { get; set; }

    public string file_url { get; set; } = null!;

    public string? public_id { get; set; }

    public string? provider { get; set; }

    public string? mime_type { get; set; }

    public int? size_bytes { get; set; }

    public int? width { get; set; }

    public int? height { get; set; }

    public DateTime? created_at { get; set; }

    public virtual MedicalDocument doc { get; set; } = null!;
}
