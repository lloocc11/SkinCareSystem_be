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
    public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<ChatMessage>> GetBySessionAsync(Guid sessionId, int pageNumber, int pageSize)
        {
            return await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.session_id == sessionId)
                .OrderBy(m => m.created_at)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountBySessionAsync(Guid sessionId)
        {
            return await _context.ChatMessages
                .AsNoTracking()
                .CountAsync(m => m.session_id == sessionId);
        }

    }
}
