namespace SkinCareSystem.Common.DTOs.Symptom
{
    public class SymptomCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ExampleImageUrl { get; set; }
    }
}
