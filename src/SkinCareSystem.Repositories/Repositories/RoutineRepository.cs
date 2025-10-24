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
    public class RoutineRepository : GenericRepository<Routine>, IRoutineRepository
    {
        public RoutineRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Routine>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.Set<Routine>()
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Analysis)
                .Include(r => r.ParentRoutine)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByUserIdAsync(Guid userId)
        {
            return await _context.Set<Routine>()
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .CountAsync();
        }

        public async Task<Routine?> GetByIdWithDetailsAsync(Guid routineId)
        {
            return await _context.Set<Routine>()
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Analysis)
                .Include(r => r.ParentRoutine)
                .Include(r => r.RoutineSteps)
                .FirstOrDefaultAsync(r => r.RoutineId == routineId);
        }
    }
}
