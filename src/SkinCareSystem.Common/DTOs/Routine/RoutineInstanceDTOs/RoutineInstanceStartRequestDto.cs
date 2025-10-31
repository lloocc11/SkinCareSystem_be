using System;

namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineInstanceStartRequestDto
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}
