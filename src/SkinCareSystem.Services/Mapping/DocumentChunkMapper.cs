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
                ChunkId = chunk.ChunkId,
                DocId = chunk.DocId,
                ChunkText = chunk.ChunkText,
                CreatedAt = chunk.CreatedAt
            };
        }

        public static DocumentChunk ToEntity(this DocumentChunkCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new DocumentChunk
            {
                ChunkId = Guid.NewGuid(),
                DocId = dto.DocId,
                ChunkText = dto.ChunkText,
                CreatedAt = DateTime.Now
            };
        }

        public static void ApplyUpdate(this DocumentChunk chunk, DocumentChunkUpdateDto dto)
        {
            if (chunk == null) throw new ArgumentNullException(nameof(chunk));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.ChunkText))
                chunk.ChunkText = dto.ChunkText;
        }
    }
}
