using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetRoleByNameAsync(string name);
}
