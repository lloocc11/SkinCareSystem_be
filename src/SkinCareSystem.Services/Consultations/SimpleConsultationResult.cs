using System;

namespace SkinCareSystem.Services.Consultations;

public sealed class SimpleConsultationResult
{
    public Guid AnalysisId { get; init; }

    public Guid RoutineId { get; init; }

    public bool RoutineGenerated { get; init; }

    public string Advice { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;

    public string Json { get; init; } = string.Empty;

    public double? Confidence { get; init; }
}
