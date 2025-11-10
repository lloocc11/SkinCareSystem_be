using System;

namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineInstanceUpdateDto
    {
        public DateOnly? EndDate { get; set; }
        public string? Status { get; set; }
        public decimal? AdherenceScore { get; set; }
    }
}
