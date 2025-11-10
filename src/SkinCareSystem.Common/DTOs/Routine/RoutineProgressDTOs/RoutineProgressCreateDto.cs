using System;

namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineProgressCreateDto
    {
        public Guid InstanceId { get; set; }
        public Guid StepId { get; set; }
        public DateTime CompletedAt { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = "completed";
    }
}
