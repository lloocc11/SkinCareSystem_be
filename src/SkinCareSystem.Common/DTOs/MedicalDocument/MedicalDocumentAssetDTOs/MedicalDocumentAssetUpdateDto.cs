namespace SkinCareSystem.Common.DTOs.MedicalDocument.MedicalDocumentAssetDTOs
{
    public class MedicalDocumentAssetUpdateDto
    {
        public string? FileUrl { get; set; }
        public string? PublicId { get; set; }
        public string? Provider { get; set; }
        public string? MimeType { get; set; }
        public int? SizeBytes { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
