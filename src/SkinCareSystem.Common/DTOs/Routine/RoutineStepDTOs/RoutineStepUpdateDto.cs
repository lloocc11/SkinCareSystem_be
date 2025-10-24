namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineStepUpdateDto
    {
        public int? StepOrder { get; set; }
        public string? Instruction { get; set; }
        public string? TimeOfDay { get; set; }
        public string? Frequency { get; set; }
    }
}
