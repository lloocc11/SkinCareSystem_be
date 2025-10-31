using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IMedicalDocumentAssetRepository : IGenericRepository<MedicalDocumentAsset>
    {
        Task<IReadOnlyList<MedicalDocumentAsset>> GetByDocumentIdAsync(Guid docId);
        Task<MedicalDocumentAsset?> GetByPublicIdAsync(string publicId);
    }
}
