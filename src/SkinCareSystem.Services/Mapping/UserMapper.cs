using System;
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

        public static User ToEntity(this UserCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new User
            {
                UserId = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email.Trim().ToLowerInvariant(),
                GoogleId = dto.GoogleId,
                RoleId = dto.RoleId,
                SkinType = dto.SkinType,
                Status = dto.Status,
                DateOfBirth = dto.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static void ApplyUpdate(this User user, UserUpdateDto dto)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user.Email = dto.Email.Trim().ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(dto.SkinType))
            {
                user.SkinType = dto.SkinType;
            }

            if (dto.DateOfBirth.HasValue)
            {
                user.DateOfBirth = dto.DateOfBirth;
            }

            if (dto.RoleId.HasValue && dto.RoleId.Value != Guid.Empty)
            {
                user.RoleId = dto.RoleId.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                user.Status = dto.Status;
            }

            user.UpdatedAt = DateTime.UtcNow;
        }
    }
}
