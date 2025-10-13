using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.IRepositories;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();
            return await _context.Set<User>()
                                 .AsNoTracking()
                                 .Include(u => u.Role)
                                 .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        }

        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            var normalizedGoogleId = googleId.Trim();
            return await _context.Set<User>()
                                 .AsNoTracking()
                                 .Include(u => u.Role)
                                 .FirstOrDefaultAsync(u => u.GoogleId == normalizedGoogleId);
        }
    }
}
