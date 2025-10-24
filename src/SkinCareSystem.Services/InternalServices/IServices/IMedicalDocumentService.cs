using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.MedicalDocument;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IMedicalDocumentService
    {
        Task<IServiceResult> GetMedicalDocumentsAsync(int pageNumber, int pageSize);
        Task<IServiceResult> GetMedicalDocumentByIdAsync(Guid docId);
        Task<IServiceResult> CreateMedicalDocumentAsync(MedicalDocumentCreateDto dto);
        Task<IServiceResult> UpdateMedicalDocumentAsync(Guid docId, MedicalDocumentUpdateDto dto);
        Task<IServiceResult> DeleteMedicalDocumentAsync(Guid docId);
    }

    public interface IDocumentChunkService
    {
        Task<IServiceResult> GetDocumentChunksByDocumentAsync(Guid docId);
        Task<IServiceResult> GetDocumentChunkByIdAsync(Guid chunkId);
        Task<IServiceResult> CreateDocumentChunkAsync(DocumentChunkCreateDto dto);
        Task<IServiceResult> UpdateDocumentChunkAsync(Guid chunkId, DocumentChunkUpdateDto dto);
        Task<IServiceResult> DeleteDocumentChunkAsync(Guid chunkId);
    }
}
