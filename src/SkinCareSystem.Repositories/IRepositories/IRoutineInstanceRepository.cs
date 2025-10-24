using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IRoutineInstanceRepository : IGenericRepository<RoutineInstance>
    {
        Task<IReadOnlyList<RoutineInstance>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> CountByUserIdAsync(Guid userId);
        Task<IReadOnlyList<RoutineInstance>> GetByRoutineIdAsync(Guid routineId);
        Task<RoutineInstance?> GetByIdWithDetailsAsync(Guid instanceId);
    }
}
