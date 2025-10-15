using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.IRepositories;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.Repositories
{
    public class UserQueryRepository : GenericRepository<UserQuery>, IUserQueryRepository
    {
        public UserQueryRepository(SkinCareSystemDbContext context) : base(context)
        {
        }
    }
}
