using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Symptom;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface ISymptomService
    {
        Task<IServiceResult> GetSymptomsAsync(int pageNumber, int pageSize);
        Task<IServiceResult> GetSymptomByIdAsync(Guid symptomId);
        Task<IServiceResult> CreateSymptomAsync(SymptomCreateDto dto);
        Task<IServiceResult> UpdateSymptomAsync(Guid symptomId, SymptomUpdateDto dto);
        Task<IServiceResult> DeleteSymptomAsync(Guid symptomId);
    }

    public interface IUserSymptomService
    {
        Task<IServiceResult> GetUserSymptomsByUserAsync(Guid userId);
        Task<IServiceResult> GetUserSymptomByIdAsync(Guid userSymptomId);
        Task<IServiceResult> CreateUserSymptomAsync(UserSymptomCreateDto dto);
        Task<IServiceResult> UpdateUserSymptomAsync(Guid userSymptomId, UserSymptomUpdateDto dto);
        Task<IServiceResult> DeleteUserSymptomAsync(Guid userSymptomId);
    }
}
