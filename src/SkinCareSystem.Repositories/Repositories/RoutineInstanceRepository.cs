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
                .Include(ri => ri.User)
                .Include(ri => ri.Routine)
                .Where(ri => ri.UserId == userId)
                .OrderByDescending(ri => ri.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByUserIdAsync(Guid userId)
        {
            return await _context.Set<RoutineInstance>()
                .AsNoTracking()
                .Where(ri => ri.UserId == userId)
                .CountAsync();
        }

        public async Task<IReadOnlyList<RoutineInstance>> GetByRoutineIdAsync(Guid routineId)
        {
            return await _context.Set<RoutineInstance>()
                .AsNoTracking()
                .Include(ri => ri.User)
                .Include(ri => ri.Routine)
                .Where(ri => ri.RoutineId == routineId)
                .OrderByDescending(ri => ri.StartDate)
                .ToListAsync();
        }

        public async Task<RoutineInstance?> GetByIdWithDetailsAsync(Guid instanceId)
        {
            return await _context.Set<RoutineInstance>()
                .AsNoTracking()
                .Include(ri => ri.User)
                .Include(ri => ri.Routine)
                .FirstOrDefaultAsync(ri => ri.InstanceId == instanceId);
        }
    }
}
