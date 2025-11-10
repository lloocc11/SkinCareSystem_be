using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinCareSystem.Services.InternalServices.IServices;

public record ChunkHit(Guid DocId, Guid ChunkId, string Content, double Score);

public interface IRagRetriever
{
    Task<IReadOnlyList<ChunkHit>> SearchAsync(string query, int k, Guid[]? restrictDocIds = null, string? embeddingModel = null);
}
