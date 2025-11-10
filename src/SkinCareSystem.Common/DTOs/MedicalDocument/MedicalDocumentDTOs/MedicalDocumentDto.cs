using System;
using System.Collections.Generic;
using SkinCareSystem.Common.DTOs.MedicalDocument.MedicalDocumentAssetDTOs;

namespace SkinCareSystem.Common.DTOs.MedicalDocument
{
    public class MedicalDocumentDto
    {
        public Guid DocId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Source { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? LastUpdated { get; set; }
        public DateTime? CreatedAt { get; set; }
        public IReadOnlyList<MedicalDocumentAssetDto> Assets { get; set; } = Array.Empty<MedicalDocumentAssetDto>();
    }
}
