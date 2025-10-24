using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SkinCareSystem.Common.DTOs.Symptom;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class UserSymptomService : IUserSymptomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserSymptomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> GetUserSymptomsByUserAsync(Guid userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "User not found"
                };
            }

            var userSymptoms = await _unitOfWork.UserSymptomRepository.GetByUserIdAsync(userId);
            var userSymptomDtos = userSymptoms.Select(us => us.ToDto()).Where(dto => dto != null).ToList();

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = userSymptomDtos
            };
        }

        public async Task<IServiceResult> GetUserSymptomByIdAsync(Guid userSymptomId)
        {
            var userSymptom = await _unitOfWork.UserSymptomRepository.GetByIdWithDetailsAsync(userSymptomId);
            if (userSymptom == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "User symptom not found"
                };
            }

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = userSymptom.ToDto()
            };
        }

        public async Task<IServiceResult> CreateUserSymptomAsync(UserSymptomCreateDto dto)
        {
            if (dto == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                };
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "User not found"
                };
            }

            var symptom = await _unitOfWork.SymptomRepository.GetByIdAsync(dto.SymptomId);
            if (symptom == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Symptom not found"
                };
            }

            var entity = dto.ToEntity();
            await _unitOfWork.UserSymptomRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = StatusCodes.Status201Created,
                Message = Const.SUCCESS_CREATE_MSG,
                Data = entity.ToDto()
            };
        }

        public async Task<IServiceResult> UpdateUserSymptomAsync(Guid userSymptomId, UserSymptomUpdateDto dto)
        {
            var userSymptom = await _unitOfWork.UserSymptomRepository.GetByIdAsync(userSymptomId);
            if (userSymptom == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "User symptom not found"
                };
            }

            userSymptom.ApplyUpdate(dto);
            await _unitOfWork.UserSymptomRepository.UpdateAsync(userSymptom);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_UPDATE_MSG,
                Data = userSymptom.ToDto()
            };
        }

        public async Task<IServiceResult> DeleteUserSymptomAsync(Guid userSymptomId)
        {
            var userSymptom = await _unitOfWork.UserSymptomRepository.GetByIdAsync(userSymptomId);
            if (userSymptom == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "User symptom not found"
                };
            }

            await _unitOfWork.UserSymptomRepository.RemoveAsync(userSymptom);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_DELETE_MSG
            };
        }
    }
}
