using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Feedback;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IFeedbackService
    {
        Task<IServiceResult> GetFeedbacksByUserAsync(Guid userId, int pageNumber, int pageSize);
        Task<IServiceResult> GetFeedbacksByRoutineAsync(Guid routineId);
        Task<IServiceResult> GetFeedbacksByStepAsync(Guid stepId);
        Task<IServiceResult> GetFeedbackByIdAsync(Guid feedbackId);
        Task<IServiceResult> CreateFeedbackAsync(FeedbackCreateDto dto);
        Task<IServiceResult> UpdateFeedbackAsync(Guid feedbackId, FeedbackUpdateDto dto);
        Task<IServiceResult> DeleteFeedbackAsync(Guid feedbackId);
    }
}
