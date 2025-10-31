using System;

namespace SkinCareSystem.Services.Consultations;

public sealed class SimpleConsultationResponse
{
    public string Advice { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;
}
