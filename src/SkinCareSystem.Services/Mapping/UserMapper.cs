using System;
using SkinCareSystem.Common.DTOs.UserDTOs;
using SkinCareSystem.Common.Utils;
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
                UserId = user.user_id,
                FullName = user.full_name,
                Email = user.email,
                GoogleId = user.google_id,
                AuthProvider = user.auth_provider,
                HasPassword = !string.IsNullOrWhiteSpace(user.password_hash),
                RoleName = user.role?.name ?? string.Empty,
                SkinType = user.skin_type,
                RoleId = user.role_id,
                Status = user.status,
                DateOfBirth = user.date_of_birth,
                CreatedAt = user.created_at,
                UpdatedAt = user.updated_at
            };
        }

        public static User ToEntity(this UserCreateDto dto, string normalizedProvider)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            normalizedProvider = string.IsNullOrWhiteSpace(normalizedProvider)
                ? "google"
                : normalizedProvider.Trim().ToLowerInvariant();

            return new User
            {
                user_id = Guid.NewGuid(),
                full_name = dto.FullName,
                email = dto.Email.Trim().ToLowerInvariant(),
                google_id = normalizedProvider == "google" ? dto.GoogleId?.Trim() : null,
                password_hash = null,
                auth_provider = normalizedProvider,
                role_id = dto.RoleId,
                skin_type = dto.SkinType,
                status = dto.Status,
                date_of_birth = dto.DateOfBirth,
                created_at = DateTimeHelper.UtcNowUnspecified(),
                updated_at = DateTimeHelper.UtcNowUnspecified()
            };
        }

        public static void ApplyUpdate(this User user, UserUpdateDto dto)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.full_name = dto.FullName;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user.email = dto.Email.Trim().ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(dto.SkinType))
            {
                user.skin_type = dto.SkinType;
            }

            if (dto.DateOfBirth.HasValue)
            {
                user.date_of_birth = dto.DateOfBirth;
            }

            if (dto.RoleId.HasValue && dto.RoleId.Value != Guid.Empty)
            {
                user.role_id = dto.RoleId.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                user.status = dto.Status;
            }

            user.updated_at = DateTimeHelper.UtcNowUnspecified();
        }
    }
}
