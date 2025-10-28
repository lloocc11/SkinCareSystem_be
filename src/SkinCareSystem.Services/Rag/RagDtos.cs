using System;
using System.Collections.Generic;

namespace SkinCareSystem.Services.Rag;

public record RagItem(
    Guid ChunkId,
    Guid DocId,
    string? Title,
    string? Source,
    string Content,
    double Distance,
    double Similarity,
    IReadOnlyList<string> AssetUrls);

public sealed class ConsultationResult
{
    public Guid AnalysisId { get; init; }

    public Guid RoutineId { get; init; }

    public string Json { get; init; } = string.Empty;

    public double? Confidence { get; init; }

    public IReadOnlyList<RagItem> ContextItems { get; init; } = Array.Empty<RagItem>();
}
