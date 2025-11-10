using System;

namespace SkinCareSystem.Common.DTOs.Feedback
{
    public class FeedbackCreateDto
    {
        public Guid RoutineId { get; set; }
        public Guid? StepId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
