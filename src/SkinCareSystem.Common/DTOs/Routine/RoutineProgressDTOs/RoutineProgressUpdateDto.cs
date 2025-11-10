using System;

namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineProgressUpdateDto
    {
        public DateTime? CompletedAt { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
    }
}
