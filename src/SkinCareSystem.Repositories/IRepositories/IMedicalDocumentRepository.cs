using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IMedicalDocumentRepository : IGenericRepository<MedicalDocument>
    {
        Task<MedicalDocument?> GetByIdWithDetailsAsync(Guid docId);
    }

    public interface IDocumentChunkRepository : IGenericRepository<DocumentChunk>
    {
        Task<IReadOnlyList<DocumentChunk>> GetByDocumentIdAsync(Guid docId);
        Task<DocumentChunk?> GetByIdWithDetailsAsync(Guid chunkId);
        Task<int> DeleteByDocumentIdAsync(Guid docId);
    }
}
