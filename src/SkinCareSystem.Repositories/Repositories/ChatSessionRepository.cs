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
    public class ChatSessionRepository : GenericRepository<ChatSession>, IChatSessionRepository
    {
        public ChatSessionRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<ChatSession?> GetByIdAsync(Guid sessionId, bool includeMessages = false)
        {
            IQueryable<ChatSession> query = _context.ChatSessions.AsNoTracking();

            if (includeMessages)
            {
                query = query.Include(s => s.ChatMessages);
            }

            return await query.FirstOrDefaultAsync(s => s.session_id == sessionId);
        }

        public async Task<IReadOnlyList<ChatSession>> GetByUserAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.ChatSessions
                .AsNoTracking()
                .Where(s => s.user_id == userId)
                .OrderByDescending(s => s.created_at)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByUserAsync(Guid userId)
        {
            return await _context.ChatSessions
                .AsNoTracking()
                .CountAsync(s => s.user_id == userId);
        }
    }
}
