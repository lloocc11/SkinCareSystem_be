using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IRoutineRepository : IGenericRepository<Routine>
    {
        Task<IReadOnlyList<Routine>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> CountByUserIdAsync(Guid userId);
        Task<Routine?> GetByIdWithDetailsAsync(Guid routineId);
    }
}
