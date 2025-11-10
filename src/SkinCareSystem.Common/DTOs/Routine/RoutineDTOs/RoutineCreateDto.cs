using System;

namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineCreateDto
    {
        public Guid UserId { get; set; }
        public Guid? AnalysisId { get; set; }
        public string? Description { get; set; }
        public Guid? ParentRoutineId { get; set; }
        public string Status { get; set; } = "active";
    }
}
