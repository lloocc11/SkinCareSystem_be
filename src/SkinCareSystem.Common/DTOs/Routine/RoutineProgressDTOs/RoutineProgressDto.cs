using System;

namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineProgressDto
    {
        public Guid ProgressId { get; set; }
        public Guid InstanceId { get; set; }
        public Guid StepId { get; set; }
        public DateTime CompletedAt { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = null!;
        public int? IrritationLevel { get; set; }
        public string? MoodNote { get; set; }
    }
}
