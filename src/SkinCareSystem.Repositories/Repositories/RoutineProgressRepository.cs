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
                .Include(rp => rp.Instance)
                .Include(rp => rp.Step)
                .Where(rp => rp.InstanceId == instanceId)
                .OrderByDescending(rp => rp.CompletedAt)
                .ToListAsync();
        }

        public async Task<RoutineProgress?> GetByIdWithDetailsAsync(Guid progressId)
        {
            return await _context.Set<RoutineProgress>()
                .AsNoTracking()
                .Include(rp => rp.Instance)
                .Include(rp => rp.Step)
                .FirstOrDefaultAsync(rp => rp.ProgressId == progressId);
        }
    }
}
