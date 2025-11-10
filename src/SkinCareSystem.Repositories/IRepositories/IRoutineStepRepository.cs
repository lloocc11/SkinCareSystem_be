using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IRoutineStepRepository : IGenericRepository<RoutineStep>
    {
        Task<IReadOnlyList<RoutineStep>> GetByRoutineIdAsync(Guid routineId);
        Task<RoutineStep?> GetByIdWithDetailsAsync(Guid stepId);
        Task<int> DeleteByRoutineIdAsync(Guid routineId);
    }
}
