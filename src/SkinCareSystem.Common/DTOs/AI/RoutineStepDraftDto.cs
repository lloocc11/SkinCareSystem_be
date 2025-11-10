namespace SkinCareSystem.Common.DTOs.AI;

public class RoutineStepDraftDto
{
    public int Order { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public string TimeOfDay { get; set; } = "morning";
    public string Frequency { get; set; } = "daily";
}
