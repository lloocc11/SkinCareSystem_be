using System;

namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineProgressLogEntryDto
    {
        public Guid ProgressId { get; set; }
        public Guid InstanceId { get; set; }
        public Guid StepId { get; set; }
        public string StepInstruction { get; set; } = string.Empty;
        public string TimeOfDay { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public DateTime CompletedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Note { get; set; }
    }
}
