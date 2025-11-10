using System;

namespace SkinCareSystem.Common.DTOs.Feedback
{
    public class FeedbackDto
    {
        public Guid FeedbackId { get; set; }
        public Guid RoutineId { get; set; }
        public Guid? StepId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UserFullName { get; set; }
    }
}
