using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IRoutineProgressRepository : IGenericRepository<RoutineProgress>
    {
        Task<IReadOnlyList<RoutineProgress>> GetByInstanceIdAsync(Guid instanceId);
        Task<RoutineProgress?> GetByIdWithDetailsAsync(Guid progressId);
        Task<RoutineProgress?> GetByInstanceStepAndDateAsync(Guid instanceId, Guid stepId, DateOnly date);
    }
}
