using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IUserService
    {
        Task<IServiceResult> GetAllUsers();
    }
}