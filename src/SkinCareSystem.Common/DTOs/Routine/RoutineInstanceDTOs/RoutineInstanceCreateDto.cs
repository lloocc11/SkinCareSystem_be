using System;

namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineInstanceCreateDto
    {
        public Guid RoutineId { get; set; }
        public Guid UserId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; } = "planned";
    }
}
