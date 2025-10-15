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
    public class AIAnalysisRepository : GenericRepository<Aianalysis>, IAIAnalysisRepository
    {
        public AIAnalysisRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<Aianalysis?> GetByMessageIdAsync(Guid messageId)
        {
            return await _context.Aianalyses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ChatMessageId == messageId);
        }

        public async Task<IReadOnlyList<Aianalysis>> GetBySessionAsync(Guid sessionId)
        {
            return await _context.Aianalyses
                .AsNoTracking()
                .Where(a => a.ChatMessage.SessionId == sessionId)
                .ToListAsync();
        }
    }
}
