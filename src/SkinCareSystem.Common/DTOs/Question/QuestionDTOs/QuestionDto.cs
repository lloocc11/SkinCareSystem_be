using System;

namespace SkinCareSystem.Common.DTOs.Question
{
    public class QuestionDto
    {
        public Guid QuestionId { get; set; }
        public string Text { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Options { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
