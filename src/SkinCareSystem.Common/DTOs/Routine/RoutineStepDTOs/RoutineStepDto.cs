using System;

namespace SkinCareSystem.Common.DTOs.Routine
{
    public class RoutineStepDto
    {
        public Guid StepId { get; set; }
        public Guid RoutineId { get; set; }
        public int StepOrder { get; set; }
        public string Instruction { get; set; } = null!;
        public string TimeOfDay { get; set; } = null!;
        public string Frequency { get; set; } = null!;
    }
}
