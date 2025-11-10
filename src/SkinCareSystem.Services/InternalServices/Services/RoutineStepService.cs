using System;
using System.Linq;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class RoutineStepService : IRoutineStepService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoutineStepService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> GetRoutineStepsByRoutineAsync(Guid routineId)
        {
            var routine = await _unitOfWork.RoutineRepository.GetByIdAsync(routineId);
            if (routine == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine not found"
                };
            }

            var steps = await _unitOfWork.RoutineStepRepository.GetByRoutineIdAsync(routineId);
            var stepDtos = steps.Select(s => s.ToDto()).Where(dto => dto != null).ToList();

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = stepDtos
            };
        }

        public async Task<IServiceResult> GetRoutineStepByIdAsync(Guid stepId)
        {
            var step = await _unitOfWork.RoutineStepRepository.GetByIdWithDetailsAsync(stepId);
            if (step == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine step not found"
                };
            }

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = step.ToDto()
            };
        }

        public async Task<IServiceResult> CreateRoutineStepAsync(RoutineStepCreateDto dto)
        {
            if (dto == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                };
            }

            var routine = await _unitOfWork.RoutineRepository.GetByIdAsync(dto.RoutineId);
            if (routine == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine not found"
                };
            }

            var entity = dto.ToEntity();
            await _unitOfWork.RoutineStepRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Microsoft.AspNetCore.Http.StatusCodes.Status201Created,
                Message = Const.SUCCESS_CREATE_MSG,
                Data = entity.ToDto()
            };
        }

        public async Task<IServiceResult> UpdateRoutineStepAsync(Guid stepId, RoutineStepUpdateDto dto)
        {
            var step = await _unitOfWork.RoutineStepRepository.GetByIdAsync(stepId);
            if (step == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine step not found"
                };
            }

            step.ApplyUpdate(dto);
            await _unitOfWork.RoutineStepRepository.UpdateAsync(step);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_UPDATE_MSG,
                Data = step.ToDto()
            };
        }

        public async Task<IServiceResult> DeleteRoutineStepAsync(Guid stepId)
        {
            var step = await _unitOfWork.RoutineStepRepository.GetByIdAsync(stepId);
            if (step == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine step not found"
                };
            }

            await _unitOfWork.RoutineStepRepository.RemoveAsync(step);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_DELETE_MSG
            };
        }
    }
}
