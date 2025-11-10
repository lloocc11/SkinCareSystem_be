using System;

namespace SkinCareSystem.Common.DTOs.MedicalDocument
{
    public class DocumentChunkCreateDto
    {
        public Guid DocId { get; set; }
        public string ChunkText { get; set; } = null!;
        public float[]? Embedding { get; set; }
    }
}
