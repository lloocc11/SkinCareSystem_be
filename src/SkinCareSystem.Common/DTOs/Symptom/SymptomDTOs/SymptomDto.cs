using System;

namespace SkinCareSystem.Common.DTOs.Symptom
{
    public class SymptomDto
    {
        public Guid SymptomId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ExampleImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
