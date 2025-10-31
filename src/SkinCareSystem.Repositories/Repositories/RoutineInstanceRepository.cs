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
    public class RoutineInstanceRepository : GenericRepository<RoutineInstance>, IRoutineInstanceRepository
    {
        public RoutineInstanceRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<RoutineInstance>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.Set<RoutineInstance>()
                .AsNoTracking()
                .Include(ri => ri.user)
                .Include(ri => ri.routine)
                .Where(ri => ri.user_id == userId)
                .OrderByDescending(ri => ri.created_at)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByUserIdAsync(Guid userId)
        {
            return await _context.Set<RoutineInstance>()
                .AsNoTracking()
                .Where(ri => ri.user_id == userId)
                .CountAsync();
        }

        public async Task<IReadOnlyList<RoutineInstance>> GetByRoutineIdAsync(Guid routineId)
        {
            return await _context.Set<RoutineInstance>()
                .AsNoTracking()
                .Include(ri => ri.user)
                .Include(ri => ri.routine)
                .Where(ri => ri.routine_id == routineId)
                .OrderByDescending(ri => ri.start_date)
                .ToListAsync();
        }

        public async Task<RoutineInstance?> GetByIdWithDetailsAsync(Guid instanceId)
        {
            return await _context.Set<RoutineInstance>()
                .AsNoTracking()
                .Include(ri => ri.user)
                .Include(ri => ri.routine)
                .FirstOrDefaultAsync(ri => ri.instance_id == instanceId);
        }
    }
}
