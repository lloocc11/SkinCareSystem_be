using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IFeedbackRepository : IGenericRepository<Feedback>
    {
        Task<IReadOnlyList<Feedback>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> CountByUserIdAsync(Guid userId);
        Task<IReadOnlyList<Feedback>> GetByRoutineIdAsync(Guid routineId);
        Task<IReadOnlyList<Feedback>> GetByStepIdAsync(Guid stepId);
        Task<Feedback?> GetByIdWithDetailsAsync(Guid feedbackId);
    }
}
