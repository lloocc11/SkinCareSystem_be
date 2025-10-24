using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface ISymptomRepository : IGenericRepository<Symptom>
    {
        Task<Symptom?> GetByIdWithDetailsAsync(Guid symptomId);
    }

    public interface IUserSymptomRepository : IGenericRepository<UserSymptom>
    {
        Task<IReadOnlyList<UserSymptom>> GetByUserIdAsync(Guid userId);
        Task<UserSymptom?> GetByIdWithDetailsAsync(Guid userSymptomId);
    }
}
