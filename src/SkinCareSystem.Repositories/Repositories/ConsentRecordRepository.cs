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
    public class ConsentRecordRepository : GenericRepository<ConsentRecord>, IConsentRecordRepository
    {
        public ConsentRecordRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<ConsentRecord>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Set<ConsentRecord>()
                .AsNoTracking()
                .Include(cr => cr.user)
                .Where(cr => cr.user_id == userId)
                .OrderByDescending(cr => cr.given_at)
                .ToListAsync();
        }

        public async Task<ConsentRecord?> GetByIdWithDetailsAsync(Guid consentId)
        {
            return await _context.Set<ConsentRecord>()
                .AsNoTracking()
                .Include(cr => cr.user)
                .FirstOrDefaultAsync(cr => cr.consent_id == consentId);
        }
    }
}
