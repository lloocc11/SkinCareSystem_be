using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class RoutineProgressService : IRoutineProgressService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoutineProgressService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> GetRoutineProgressByInstanceAsync(Guid instanceId)
        {
            var instance = await _unitOfWork.RoutineInstanceRepository.GetByIdAsync(instanceId);
            if (instance == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine instance not found"
                };
            }

            var progresses = await _unitOfWork.RoutineProgressRepository.GetByInstanceIdAsync(instanceId);
            var progressDtos = progresses.Select(p => p.ToDto()).Where(dto => dto != null).ToList();

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = progressDtos
            };
        }

        public async Task<IServiceResult> GetRoutineProgressByIdAsync(Guid progressId)
        {
            var progress = await _unitOfWork.RoutineProgressRepository.GetByIdWithDetailsAsync(progressId);
            if (progress == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine progress not found"
                };
            }

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = progress.ToDto()
            };
        }

        public async Task<IServiceResult> CreateRoutineProgressAsync(RoutineProgressCreateDto dto)
        {
            if (dto == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                };
            }

            var instance = await _unitOfWork.RoutineInstanceRepository.GetByIdAsync(dto.InstanceId);
            if (instance == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine instance not found"
                };
            }

            var step = await _unitOfWork.RoutineStepRepository.GetByIdAsync(dto.StepId);
            if (step == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine step not found"
                };
            }

            var entity = dto.ToEntity();
            await _unitOfWork.RoutineProgressRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = StatusCodes.Status201Created,
                Message = Const.SUCCESS_CREATE_MSG,
                Data = entity.ToDto()
            };
        }

        public async Task<IServiceResult> UpdateRoutineProgressAsync(Guid progressId, RoutineProgressUpdateDto dto)
        {
            var progress = await _unitOfWork.RoutineProgressRepository.GetByIdAsync(progressId);
            if (progress == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine progress not found"
                };
            }

            progress.ApplyUpdate(dto);
            await _unitOfWork.RoutineProgressRepository.UpdateAsync(progress);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_UPDATE_MSG,
                Data = progress.ToDto()
            };
        }

        public async Task<IServiceResult> DeleteRoutineProgressAsync(Guid progressId)
        {
            var progress = await _unitOfWork.RoutineProgressRepository.GetByIdAsync(progressId);
            if (progress == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine progress not found"
                };
            }

            await _unitOfWork.RoutineProgressRepository.RemoveAsync(progress);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_DELETE_MSG
            };
        }
    }
}
