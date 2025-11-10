using System;

namespace SkinCareSystem.Common.DTOs.MedicalDocument;

public class IngestDocumentResponseDto
{
    public Guid DocumentId { get; set; }
    public int AssetCount { get; set; }
    public string IngestStatus { get; set; } = "queued";
}
