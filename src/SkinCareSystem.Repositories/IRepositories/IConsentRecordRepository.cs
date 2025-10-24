using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IConsentRecordRepository : IGenericRepository<ConsentRecord>
    {
        Task<IReadOnlyList<ConsentRecord>> GetByUserIdAsync(Guid userId);
        Task<ConsentRecord?> GetByIdWithDetailsAsync(Guid consentId);
    }
}
