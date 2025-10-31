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
    public class RoutineProgressRepository : GenericRepository<RoutineProgress>, IRoutineProgressRepository
    {
        public RoutineProgressRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<RoutineProgress>> GetByInstanceIdAsync(Guid instanceId)
        {
            return await _context.Set<RoutineProgress>()
                .AsNoTracking()
                .Include(rp => rp.instance)
                .Include(rp => rp.step)
                .Where(rp => rp.instance_id == instanceId)
                .OrderByDescending(rp => rp.completed_at)
                .ToListAsync();
        }

        public async Task<RoutineProgress?> GetByIdWithDetailsAsync(Guid progressId)
        {
            return await _context.Set<RoutineProgress>()
                .AsNoTracking()
                .Include(rp => rp.instance)
                .Include(rp => rp.step)
                .FirstOrDefaultAsync(rp => rp.progress_id == progressId);
        }

        public async Task<RoutineProgress?> GetByInstanceStepAndDateAsync(Guid instanceId, Guid stepId, DateOnly date)
        {
            var startOfDay = date.ToDateTime(TimeOnly.MinValue);
            var endOfDay = startOfDay.AddDays(1);

            return await _context.Set<RoutineProgress>()
                .AsNoTracking()
                .Where(rp => rp.instance_id == instanceId
                             && rp.step_id == stepId
                             && rp.completed_at >= startOfDay
                             && rp.completed_at < endOfDay)
                .OrderByDescending(rp => rp.completed_at)
                .FirstOrDefaultAsync();
        }
    }
}
