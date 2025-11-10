namespace SkinCareSystem.Common.DTOs.MedicalDocument
{
    public class DocumentChunkUpdateDto
    {
        public string? ChunkText { get; set; }
        public float[]? Embedding { get; set; }
    }
}
