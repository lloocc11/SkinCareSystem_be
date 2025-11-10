namespace SkinCareSystem.Common.DTOs.MedicalDocument;

public class EmbedDocumentRequestDto
{
    public int ChunkSize { get; set; } = 1000;
    public int ChunkOverlap { get; set; } = 150;
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
}

public class EmbedDocumentResponseDto
{
    public int ChunkCount { get; set; }
}
