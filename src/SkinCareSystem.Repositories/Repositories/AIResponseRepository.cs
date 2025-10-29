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
    public class AIResponseRepository : GenericRepository<AIResponse>, IAIResponseRepository
    {
        public AIResponseRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<AIResponse>> GetByQueryAsync(Guid queryId)
        {
            return await _context.AIResponses
                .AsNoTracking()
                .Where(r => r.query_id == queryId)
                .OrderBy(r => r.created_at)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<AIResponse>> GetByQueryIdsAsync(IEnumerable<Guid> queryIds)
        {
            var ids = queryIds?.ToList() ?? new List<Guid>();
            if (ids.Count == 0)
            {
                return Array.Empty<AIResponse>();
            }

            return await _context.AIResponses
                .AsNoTracking()
                .Where(r => ids.Contains(r.query_id))
                .OrderBy(r => r.created_at)
                .ToListAsync();
        }

    }
}
