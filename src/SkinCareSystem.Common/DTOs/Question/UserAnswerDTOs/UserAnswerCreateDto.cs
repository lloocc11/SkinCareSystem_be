using System;

namespace SkinCareSystem.Common.DTOs.Question
{
    public class UserAnswerCreateDto
    {
        public Guid UserId { get; set; }
        public Guid QuestionId { get; set; }
        public string AnswerValue { get; set; } = null!;
    }
}
