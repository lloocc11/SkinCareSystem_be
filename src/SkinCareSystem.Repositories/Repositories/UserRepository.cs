using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.IRepositores;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository()
        {
        }
        public UserRepository(SkinCareSystemDbContext context) 
            => _context = context;
        
        
    }
}