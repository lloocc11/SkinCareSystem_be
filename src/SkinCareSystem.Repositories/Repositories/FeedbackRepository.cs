using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.IRepositories;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.Repositories
{
    public class FeedbackRepository : GenericRepository<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Feedback>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.Set<Feedback>()
                .AsNoTracking()
                .Include(f => f.user)
                .Include(f => f.routine)
                .Include(f => f.step)
                .Where(f => f.user_id == userId)
                .OrderByDescending(f => f.created_at)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByUserIdAsync(Guid userId)
        {
            return await _context.Set<Feedback>()
                .AsNoTracking()
                .Where(f => f.user_id == userId)
                .CountAsync();
        }

        public async Task<IReadOnlyList<Feedback>> GetByRoutineIdAsync(Guid routineId)
        {
            return await _context.Set<Feedback>()
                .AsNoTracking()
                .Include(f => f.user)
                .Include(f => f.routine)
                .Include(f => f.step)
                .Where(f => f.routine_id == routineId)
                .OrderByDescending(f => f.created_at)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Feedback>> GetByStepIdAsync(Guid stepId)
        {
            return await _context.Set<Feedback>()
                .AsNoTracking()
                .Include(f => f.user)
                .Include(f => f.routine)
                .Include(f => f.step)
                .Where(f => f.step_id == stepId)
                .OrderByDescending(f => f.created_at)
                .ToListAsync();
        }

        public async Task<Feedback?> GetByIdWithDetailsAsync(Guid feedbackId)
        {
            return await _context.Set<Feedback>()
                .AsNoTracking()
                .Include(f => f.user)
                .Include(f => f.routine)
                .Include(f => f.step)
                .FirstOrDefaultAsync(f => f.feedback_id == feedbackId);
        }
    }
}
