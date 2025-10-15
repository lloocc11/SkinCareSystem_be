using System;
using SkinCareSystem.Common.DTOs.UserDTOs;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IUserService
    {
        Task<IServiceResult> GetUsersAsync(int pageNumber, int pageSize);
        Task<IServiceResult> GetUserByIdAsync(Guid userId);
        Task<IServiceResult> CreateUserAsync(UserCreateDto dto);
        Task<IServiceResult> UpdateUserAsync(Guid userId, UserUpdateDto dto);
        Task<IServiceResult> SoftDeleteUserAsync(Guid userId);
    }
}
