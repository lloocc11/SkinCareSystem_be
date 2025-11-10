namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineUpdateDto
    {
        public string? Description { get; set; }
        public string? TargetSkinType { get; set; }
        public string? TargetConditions { get; set; }
        public string? RoutineType { get; set; }
        public int? Version { get; set; }
        public string? Status { get; set; }
    }
}
