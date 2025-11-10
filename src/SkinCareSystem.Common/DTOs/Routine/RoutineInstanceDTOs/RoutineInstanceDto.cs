using System;

namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineInstanceDto
    {
        public Guid InstanceId { get; set; }
        public Guid RoutineId { get; set; }
        public Guid UserId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public decimal? AdherenceScore { get; set; }
        public string? UserFullName { get; set; }
    }
}
