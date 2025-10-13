using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        

        public async Task<IServiceResult> GetAllUsers()
        {
            try
            {
                var users = await _unitOfWork.UserRepository.GetAllAsync();
                if (users == null || !users.Any())
                {
                    return new ServiceResult
                    {
                        Status = Const.WARNING_NO_DATA_CODE,
                        Message = Const.WARNING_NO_DATA_MSG
                    };
                }

                var userDtos = users
                    .Select(user => user.ToUserDto())
                    .Where(dto => dto != null)
                    .Cast<UserDto>()
                    .ToList();

                return new ServiceResult
                {
                    Status = Const.SUCCESS_READ_CODE,
                    Message = Const.SUCCESS_READ_MSG,
                    Data = userDtos
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
    }
}
