using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.IRepositories;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<Role?> GetRoleByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var normalizedName = name.Trim().ToLower();
            return await _context.Set<Role>()
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(r => r.Name.ToLower() == normalizedName);
        }
    }
}
