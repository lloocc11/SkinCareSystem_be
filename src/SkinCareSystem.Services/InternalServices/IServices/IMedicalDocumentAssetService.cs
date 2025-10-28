using System;
using System.Threading;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.MedicalDocument.MedicalDocumentAssetDTOs;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IMedicalDocumentAssetService
    {
        Task<IServiceResult> GetAssetByIdAsync(Guid assetId);
        Task<IServiceResult> GetAssetsByDocumentAsync(Guid documentId);
        Task<IServiceResult> CreateAssetAsync(MedicalDocumentAssetCreateDto dto, CancellationToken cancellationToken = default);
        Task<IServiceResult> UpdateAssetAsync(Guid assetId, MedicalDocumentAssetUpdateDto dto, CancellationToken cancellationToken = default);
        Task<IServiceResult> DeleteAssetAsync(Guid assetId, CancellationToken cancellationToken = default);
    }
}
