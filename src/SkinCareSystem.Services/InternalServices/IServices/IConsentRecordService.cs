using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Consent;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IConsentRecordService
    {
        Task<IServiceResult> GetConsentRecordsByUserAsync(Guid userId);
        Task<IServiceResult> GetConsentRecordByIdAsync(Guid consentId);
        Task<IServiceResult> CreateConsentRecordAsync(ConsentRecordCreateDto dto);
        Task<IServiceResult> UpdateConsentRecordAsync(Guid consentId, ConsentRecordUpdateDto dto);
        Task<IServiceResult> DeleteConsentRecordAsync(Guid consentId);
    }
}
