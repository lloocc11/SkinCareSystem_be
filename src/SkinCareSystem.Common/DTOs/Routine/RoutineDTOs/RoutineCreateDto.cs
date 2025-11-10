using System;

namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineCreateDto
    {
        public Guid UserId { get; set; }
        public Guid? AnalysisId { get; set; }
        public string? Description { get; set; }
        public Guid? ParentRoutineId { get; set; }
        public string? TargetSkinType { get; set; }
        public string? TargetConditions { get; set; }
        public string RoutineType { get; set; } = "template";
        public int Version { get; set; } = 1;
        public string Status { get; set; } = "draft";
    }
}
