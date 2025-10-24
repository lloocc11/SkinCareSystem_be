using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Pagination;
using SkinCareSystem.Common.DTOs.Question;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public QuestionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IServiceResult> GetQuestionsAsync(int pageNumber, int pageSize)
        {
            try
            {
                var allQuestions = await _unitOfWork.Questions.GetAllAsync();
                var totalRecords = allQuestions.Count();

                var pagedQuestions = allQuestions
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(q => q.ToDto())
                    .ToList();

                var pagedResult = new PagedResult<QuestionDto>
                {
                    Items = pagedQuestions!,
                    TotalItems = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
                };

                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, pagedResult);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> GetQuestionByIdAsync(Guid questionId)
        {
            try
            {
                var question = await _unitOfWork.Questions.GetByIdAsync(questionId);
                if (question == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, question.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> CreateQuestionAsync(QuestionCreateDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return new ServiceResult(Const.FAIL_CREATE_CODE, "Invalid question data");
                }

                var question = dto.ToEntity();
                await _unitOfWork.Questions.CreateAsync(question);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, question.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> UpdateQuestionAsync(Guid questionId, QuestionUpdateDto dto)
        {
            try
            {
                var question = await _unitOfWork.Questions.GetByIdAsync(questionId);
                if (question == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                question.ApplyUpdate(dto);
                await _unitOfWork.Questions.UpdateAsync(question);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, question.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> DeleteQuestionAsync(Guid questionId)
        {
            try
            {
                var question = await _unitOfWork.Questions.GetByIdAsync(questionId);
                if (question == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                await _unitOfWork.Questions.RemoveAsync(question);
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
