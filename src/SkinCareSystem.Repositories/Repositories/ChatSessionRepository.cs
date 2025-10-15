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

            return await query.FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }

        public async Task<IReadOnlyList<ChatSession>> GetByUserAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.ChatSessions
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByUserAsync(Guid userId)
        {
            return await _context.ChatSessions
                .AsNoTracking()
                .CountAsync(s => s.UserId == userId);
        }
    }
}
