using System;

namespace SkinCareSystem.Common.DTOs.MedicalDocument
{
    public class DocumentChunkDto
    {
        public Guid ChunkId { get; set; }
        public Guid DocId { get; set; }
        public string ChunkText { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }
}
