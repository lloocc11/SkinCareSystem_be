using System;

namespace SkinCareSystem.Common.DTOs.MedicalDocument.MedicalDocumentAssetDTOs
{
    public class MedicalDocumentAssetDto
    {
        public Guid AssetId { get; set; }
        public Guid DocId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string? PublicId { get; set; }
        public string? Provider { get; set; }
        public string? MimeType { get; set; }
        public int? SizeBytes { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
