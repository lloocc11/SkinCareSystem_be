namespace SkinCareSystem.Common.DTOs.MedicalDocument
{
    public class MedicalDocumentCreateDto
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Source { get; set; }
        public string Status { get; set; } = "active";
    }
}
