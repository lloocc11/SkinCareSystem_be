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
    public class RoutineStepRepository : GenericRepository<RoutineStep>, IRoutineStepRepository
    {
        public RoutineStepRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<RoutineStep>> GetByRoutineIdAsync(Guid routineId)
        {
            return await _context.Set<RoutineStep>()
                .AsNoTracking()
                .Include(rs => rs.routine)
                .Where(rs => rs.routine_id == routineId)
                .OrderBy(rs => rs.step_order)
                .ToListAsync();
        }

        public async Task<RoutineStep?> GetByIdWithDetailsAsync(Guid stepId)
        {
            return await _context.Set<RoutineStep>()
                .AsNoTracking()
                .Include(rs => rs.routine)
                .FirstOrDefaultAsync(rs => rs.step_id == stepId);
        }

        public async Task<int> DeleteByRoutineIdAsync(Guid routineId)
        {
            return await _context.Set<RoutineStep>()
                .Where(rs => rs.routine_id == routineId)
                .ExecuteDeleteAsync();
        }
    }
}
