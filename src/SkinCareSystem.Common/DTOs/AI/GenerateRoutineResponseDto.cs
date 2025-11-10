using System;
using System.Collections.Generic;

namespace SkinCareSystem.Common.DTOs.AI;

public class RoutineCitationDto
{
    public Guid DocId { get; set; }
    public Guid ChunkId { get; set; }
    public double Score { get; set; }
}

public class GenerateRoutineResponseDto
{
    public Guid? RoutineId { get; set; }
    public RoutineDraftDto Routine { get; set; } = new();
    public IList<RoutineCitationDto> Citations { get; set; } = new List<RoutineCitationDto>();
    public bool IsRagBased { get; set; }
    public string Source { get; set; } = "llm";
}
