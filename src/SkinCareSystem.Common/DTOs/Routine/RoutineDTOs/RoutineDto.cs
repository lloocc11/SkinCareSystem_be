using System;

namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineDto
    {
        public Guid RoutineId { get; set; }
        public Guid UserId { get; set; }
        public Guid? AnalysisId { get; set; }
        public string? Description { get; set; }
        public int? Version { get; set; }
        public Guid? ParentRoutineId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UserFullName { get; set; }
    }
}
