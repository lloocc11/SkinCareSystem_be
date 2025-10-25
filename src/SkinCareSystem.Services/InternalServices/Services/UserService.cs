using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Common.DTOs.Pagination;
using SkinCareSystem.Common.DTOs.UserDTOs;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class UserService: IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }
        

        public async Task<IServiceResult> GetUsersAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                };
            }

            try
            {
                var query = _unitOfWork.UserRepository.GetAllQueryable()
                    .OrderBy(u => u.FullName);

                var totalItems = await query.CountAsync();
                if (totalItems == 0)
                {
                    return new ServiceResult
                    {
                        Status = Const.WARNING_NO_DATA_CODE,
                        Message = Const.WARNING_NO_DATA_MSG
                    };
                }

                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var users = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userDtos = users
                    .Select(user => user.ToUserDto())
                    .Where(dto => dto != null)
                    .Select(dto => dto!)
                    .ToList();

                var pagedResult = new PagedResult<UserDto>
                {
                    Items = userDtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                return new ServiceResult
                {
                    Status = Const.SUCCESS_READ_CODE,
                    Message = Const.SUCCESS_READ_MSG,
                    Data = pagedResult
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_EXCEPTION,
                    Message = ex.Message
                };
            }
        }

        public async Task<IServiceResult> GetUserByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "User not found"
                };
            }

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = user.ToUserDto()
            };
        }

        public async Task<IServiceResult> CreateUserAsync(UserCreateDto dto)
        {
            if (dto == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                };
            }

            var email = dto.Email.Trim().ToLowerInvariant();
            var existingByEmail = await _unitOfWork.UserRepository.GetByEmailAsync(email);
            if (existingByEmail != null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_DATA_EXISTED_CODE,
                    Message = Const.ERROR_EMAIL_EXISTS_MSG
                };
            }

            var existingByGoogle = await _unitOfWork.UserRepository.GetByGoogleIdAsync(dto.GoogleId);
            if (existingByGoogle != null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_DATA_EXISTED_CODE,
                    Message = "Google account already registered"
                };
            }

            var role = await _unitOfWork.RoleRepository.GetByIdAsync(dto.RoleId);
            if (role == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Role not found"
                };
            }

            var entity = dto.ToEntity();
            entity.Role = role;

            await _unitOfWork.UserRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = StatusCodes.Status201Created,
                Message = Const.SUCCESS_CREATE_MSG,
                Data = entity.ToUserDto()
            };
        }

        public async Task<IServiceResult> UpdateUserAsync(Guid userId, UserUpdateDto dto)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "User not found"
                };
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var normalized = dto.Email.Trim().ToLowerInvariant();
                var existing = await _unitOfWork.UserRepository.GetByEmailAsync(normalized);
                if (existing != null && existing.UserId != userId)
                {
                    return new ServiceResult
                    {
                        Status = Const.WARNING_DATA_EXISTED_CODE,
                        Message = Const.ERROR_EMAIL_EXISTS_MSG
                    };
                }
            }

            user.ApplyUpdate(dto);

            if (dto.RoleId.HasValue)
            {
                var role = await _unitOfWork.RoleRepository.GetByIdAsync(dto.RoleId.Value);
                if (role == null)
                {
                    return new ServiceResult
                    {
                        Status = Const.ERROR_VALIDATION_CODE,
                        Message = "Role not found"
                    };
                }

                user.Role = role;
            }

            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_UPDATE_MSG,
                Data = user.ToUserDto()
            };
        }

        public async Task<IServiceResult> SoftDeleteUserAsync(Guid userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "User not found"
                };
            }

            user.Status = "inactive";
            user.UpdatedAt = DateTime.Now;

            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_DELETE_MSG,
                Data = user.ToUserDto()
            };
        }
    }
}
