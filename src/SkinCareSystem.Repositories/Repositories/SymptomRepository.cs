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
                .FirstOrDefaultAsync(s => s.SymptomId == symptomId);
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
                .Include(us => us.User)
                .Include(us => us.Symptom)
                .Where(us => us.UserId == userId)
                .OrderByDescending(us => us.ReportedAt)
                .ToListAsync();
        }

        public async Task<UserSymptom?> GetByIdWithDetailsAsync(Guid userSymptomId)
        {
            return await _context.Set<UserSymptom>()
                .AsNoTracking()
                .Include(us => us.User)
                .Include(us => us.Symptom)
                .FirstOrDefaultAsync(us => us.UserSymptomId == userSymptomId);
        }
    }
}
