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
    public class AIAnalysisRepository : GenericRepository<AIAnalysis>, IAIAnalysisRepository
    {
        public AIAnalysisRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<AIAnalysis?> GetByMessageIdAsync(Guid messageId)
        {
            return await _context.AIAnalyses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.chat_message_id == messageId);
        }

        public async Task<IReadOnlyList<AIAnalysis>> GetBySessionAsync(Guid sessionId)
        {
            return await _context.AIAnalyses
                .AsNoTracking()
                .Where(a => a.chat_message.session_id == sessionId)
                .ToListAsync();
        }
    }
}
