using SkinCareSystem.Common.DTOs.UserDTOs;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class UserMapper
    {
        public static UserDto? ToUserDto(this User user)
        {
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                GoogleId = user.GoogleId,
                RoleName = user.Role?.Name ?? string.Empty,
                SkinType = user.SkinType,
                RoleId = user.RoleId,
                Status = user.Status,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
