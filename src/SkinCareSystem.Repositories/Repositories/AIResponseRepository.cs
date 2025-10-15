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
    public class AIResponseRepository : GenericRepository<Airesponse>, IAIResponseRepository
    {
        public AIResponseRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Airesponse>> GetByQueryAsync(Guid queryId)
        {
            return await _context.Airesponses
                .AsNoTracking()
                .Where(r => r.QueryId == queryId)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Airesponse>> GetByQueryIdsAsync(IEnumerable<Guid> queryIds)
        {
            var ids = queryIds?.ToList() ?? new List<Guid>();
            if (ids.Count == 0)
            {
                return Array.Empty<Airesponse>();
            }

            return await _context.Airesponses
                .AsNoTracking()
                .Where(r => ids.Contains(r.QueryId))
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

    }
}
