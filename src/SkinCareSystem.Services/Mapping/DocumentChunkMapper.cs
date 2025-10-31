using System;
using SkinCareSystem.Common.DTOs.MedicalDocument;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class DocumentChunkMapper
    {
        public static DocumentChunkDto? ToDto(this DocumentChunk chunk)
        {
            if (chunk == null) return null;

            return new DocumentChunkDto
            {
                ChunkId = chunk.chunk_id,
                DocId = chunk.doc_id,
                ChunkText = chunk.chunk_text,
                CreatedAt = chunk.created_at
            };
        }

        public static DocumentChunk ToEntity(this DocumentChunkCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new DocumentChunk
            {
                chunk_id = Guid.NewGuid(),
                doc_id = dto.DocId,
                chunk_text = dto.ChunkText,
                created_at = DateTime.Now
            };
        }

        public static void ApplyUpdate(this DocumentChunk chunk, DocumentChunkUpdateDto dto)
        {
            if (chunk == null) throw new ArgumentNullException(nameof(chunk));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.ChunkText))
                chunk.chunk_text = dto.ChunkText;
        }
    }
}
