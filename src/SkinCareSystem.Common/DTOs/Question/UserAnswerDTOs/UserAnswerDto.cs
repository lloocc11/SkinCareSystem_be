using System;

namespace SkinCareSystem.Common.DTOs.Question
{
    public class UserAnswerDto
    {
        public Guid AnswerId { get; set; }
        public Guid UserId { get; set; }
        public Guid QuestionId { get; set; }
        public string AnswerValue { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UserFullName { get; set; }
        public string? QuestionText { get; set; }
    }
}
