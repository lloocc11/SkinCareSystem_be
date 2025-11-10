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
                .Include(r => r.user)
                .Include(r => r.analysis)
                .Include(r => r.parent_routine)
                .Where(r => r.user_id == userId)
                .OrderByDescending(r => r.created_at)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByUserIdAsync(Guid userId)
        {
            return await _context.Set<Routine>()
                .AsNoTracking()
                .Where(r => r.user_id == userId)
                .CountAsync();
        }

        public async Task<Routine?> GetByIdWithDetailsAsync(Guid routineId)
        {
            return await _context.Set<Routine>()
                .AsNoTracking()
                .Include(r => r.user)
                .Include(r => r.analysis)
                .Include(r => r.parent_routine)
                .Include(r => r.RoutineSteps)
                .FirstOrDefaultAsync(r => r.routine_id == routineId);
        }

        public async Task<IReadOnlyList<Routine>> GetPublishedTemplatesAsync(string? targetSkinType = null)
        {
            var query = _context.Set<Routine>()
                .AsNoTracking()
                .Include(r => r.RoutineSteps)
                .Where(r => r.routine_type == "template" && r.status == "published");

            if (!string.IsNullOrWhiteSpace(targetSkinType))
            {
                var normalized = targetSkinType.Trim().ToLower();
                query = query.Where(r =>
                    r.target_skin_type == null ||
                    r.target_skin_type.ToLower() == normalized);
            }

            return await query
                .OrderByDescending(r => r.updated_at ?? r.created_at)
                .Take(100)
                .ToListAsync();
        }
    }
}
