using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Question;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IQuestionService
    {
        Task<IServiceResult> GetQuestionsAsync(int pageNumber, int pageSize);
        Task<IServiceResult> GetQuestionByIdAsync(Guid questionId);
        Task<IServiceResult> CreateQuestionAsync(QuestionCreateDto dto);
        Task<IServiceResult> UpdateQuestionAsync(Guid questionId, QuestionUpdateDto dto);
        Task<IServiceResult> DeleteQuestionAsync(Guid questionId);
    }

    public interface IUserAnswerService
    {
        Task<IServiceResult> GetUserAnswersByUserAsync(Guid userId);
        Task<IServiceResult> GetUserAnswerByIdAsync(Guid answerId);
        Task<IServiceResult> CreateUserAnswerAsync(UserAnswerCreateDto dto);
        Task<IServiceResult> UpdateUserAnswerAsync(Guid answerId, UserAnswerUpdateDto dto);
        Task<IServiceResult> DeleteUserAnswerAsync(Guid answerId);
    }
}
