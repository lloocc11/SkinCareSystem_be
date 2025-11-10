using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SkinCareSystem.Common.DTOs.Feedback;
using SkinCareSystem.Common.DTOs.Pagination;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeedbackService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> GetFeedbacksByUserAsync(Guid userId, int pageNumber, int pageSize)
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
                var totalItems = await _unitOfWork.FeedbackRepository.CountByUserIdAsync(userId);
                if (totalItems == 0)
                {
                    return new ServiceResult
                    {
                        Status = Const.WARNING_NO_DATA_CODE,
                        Message = Const.WARNING_NO_DATA_MSG
                    };
                }

                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var feedbacks = await _unitOfWork.FeedbackRepository.GetByUserIdAsync(userId, pageNumber, pageSize);

                var feedbackDtos = feedbacks
                    .Select(f => f.ToDto())
                    .Where(dto => dto != null)
                    .Select(dto => dto!)
                    .ToList();

                var pagedResult = new PagedResult<FeedbackDto>
                {
                    Items = feedbackDtos,
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

        public async Task<IServiceResult> GetFeedbacksByRoutineAsync(Guid routineId)
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

            var feedbacks = await _unitOfWork.FeedbackRepository.GetByRoutineIdAsync(routineId);
            var feedbackDtos = feedbacks.Select(f => f.ToDto()).Where(dto => dto != null).ToList();

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = feedbackDtos
            };
        }

        public async Task<IServiceResult> GetFeedbacksByStepAsync(Guid stepId)
        {
            var step = await _unitOfWork.RoutineStepRepository.GetByIdAsync(stepId);
            if (step == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine step not found"
                };
            }

            var feedbacks = await _unitOfWork.FeedbackRepository.GetByStepIdAsync(stepId);
            var feedbackDtos = feedbacks.Select(f => f.ToDto()).Where(dto => dto != null).ToList();

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = feedbackDtos
            };
        }

        public async Task<IServiceResult> GetFeedbackByIdAsync(Guid feedbackId)
        {
            var feedback = await _unitOfWork.FeedbackRepository.GetByIdWithDetailsAsync(feedbackId);
            if (feedback == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Feedback not found"
                };
            }

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = feedback.ToDto()
            };
        }

        public async Task<IServiceResult> CreateFeedbackAsync(FeedbackCreateDto dto)
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

            var routine = await _unitOfWork.RoutineRepository.GetByIdAsync(dto.RoutineId);
            if (routine == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine not found"
                };
            }

            if (dto.StepId.HasValue)
            {
                var step = await _unitOfWork.RoutineStepRepository.GetByIdAsync(dto.StepId.Value);
                if (step == null)
                {
                    return new ServiceResult
                    {
                        Status = Const.ERROR_VALIDATION_CODE,
                        Message = "Routine step not found"
                    };
                }
            }

            var entity = dto.ToEntity();
            await _unitOfWork.FeedbackRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = StatusCodes.Status201Created,
                Message = Const.SUCCESS_CREATE_MSG,
                Data = entity.ToDto()
            };
        }

        public async Task<IServiceResult> UpdateFeedbackAsync(Guid feedbackId, FeedbackUpdateDto dto)
        {
            var feedback = await _unitOfWork.FeedbackRepository.GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Feedback not found"
                };
            }

            feedback.ApplyUpdate(dto);
            await _unitOfWork.FeedbackRepository.UpdateAsync(feedback);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_UPDATE_MSG,
                Data = feedback.ToDto()
            };
        }

        public async Task<IServiceResult> DeleteFeedbackAsync(Guid feedbackId)
        {
            var feedback = await _unitOfWork.FeedbackRepository.GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Feedback not found"
                };
            }

            await _unitOfWork.FeedbackRepository.RemoveAsync(feedback);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_DELETE_MSG
            };
        }
    }
}
