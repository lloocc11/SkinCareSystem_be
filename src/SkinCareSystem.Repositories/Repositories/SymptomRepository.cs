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
    public class SymptomRepository : GenericRepository<Symptom>, ISymptomRepository
    {
        public SymptomRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<Symptom?> GetByIdWithDetailsAsync(Guid symptomId)
        {
            return await _context.Set<Symptom>()
                .AsNoTracking()
                .Include(s => s.UserSymptoms)
                .FirstOrDefaultAsync(s => s.symptom_id == symptomId);
        }
    }

    public class UserSymptomRepository : GenericRepository<UserSymptom>, IUserSymptomRepository
    {
        public UserSymptomRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<UserSymptom>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Set<UserSymptom>()
                .AsNoTracking()
                .Include(us => us.user)
                .Include(us => us.symptom)
                .Where(us => us.user_id == userId)
                .OrderByDescending(us => us.reported_at)
                .ToListAsync();
        }

        public async Task<UserSymptom?> GetByIdWithDetailsAsync(Guid userSymptomId)
        {
            return await _context.Set<UserSymptom>()
                .AsNoTracking()
                .Include(us => us.user)
                .Include(us => us.symptom)
                .FirstOrDefaultAsync(us => us.user_symptom_id == userSymptomId);
        }
    }
}
