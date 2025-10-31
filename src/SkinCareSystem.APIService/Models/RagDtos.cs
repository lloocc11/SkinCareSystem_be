using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.APIService.Models;

public sealed class RagSearchRequest
{
    [Required]
    public string Query { get; set; } = string.Empty;

    public int TopK { get; set; } = 6;

    public string[]? SourceFilter { get; set; }
}

public sealed record RagItemDto(
    Guid ChunkId,
    Guid DocId,
    string? Title,
    string? Source,
    string Content,
    double Distance,
    double Similarity,
    string[] AssetUrls)
{
    public static RagItemDto FromDomain(RagItem item) =>
        new(
            item.ChunkId,
            item.DocId,
            item.Title,
            item.Source,
            item.Content,
            item.Distance,
            item.Similarity,
            item.AssetUrls?.ToArray() ?? Array.Empty<string>());
}

public sealed class ConsultationForm
{
    [Required]
    public string Text { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public string? ImageUrl { get; set; }

    public bool GenerateRoutine { get; set; }
}
