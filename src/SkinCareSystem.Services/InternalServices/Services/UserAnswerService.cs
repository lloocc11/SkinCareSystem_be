using SkinCareSystem.Common.DTOs.Question;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class UserAnswerService : IUserAnswerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserAnswerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IServiceResult> GetUserAnswersByUserAsync(Guid userId)
        {
            try
            {
                var answers = await _unitOfWork.UserAnswers.GetByUserIdAsync(userId);
                var answerDtos = answers.Select(a => a.ToDto()).ToList();
                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, answerDtos);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> GetUserAnswerByIdAsync(Guid answerId)
        {
            try
            {
                var answer = await _unitOfWork.UserAnswers.GetByIdAsync(answerId);
                if (answer == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, answer.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> CreateUserAnswerAsync(UserAnswerCreateDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return new ServiceResult(Const.FAIL_CREATE_CODE, "Invalid answer data");
                }

                var answer = dto.ToEntity();
                await _unitOfWork.UserAnswers.CreateAsync(answer);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, answer.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> UpdateUserAnswerAsync(Guid answerId, UserAnswerUpdateDto dto)
        {
            try
            {
                var answer = await _unitOfWork.UserAnswers.GetByIdAsync(answerId);
                if (answer == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                answer.ApplyUpdate(dto);
                await _unitOfWork.UserAnswers.UpdateAsync(answer);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, answer.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> DeleteUserAnswerAsync(Guid answerId)
        {
            try
            {
                var answer = await _unitOfWork.UserAnswers.GetByIdAsync(answerId);
                if (answer == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                await _unitOfWork.UserAnswers.RemoveAsync(answer);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }
    }
}
