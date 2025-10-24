using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.DTOs.Feedback;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing feedback on routines and routine steps
    /// </summary>
    [Route("api/feedbacks")]
    [Authorize]
    public class FeedbacksController : BaseApiController
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbacksController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));
        }

        /// <summary>
        /// Get all feedbacks for a specific user with pagination
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            if (!isAdmin && (!Guid.TryParse(userIdClaim, out var requesterId) || requesterId != userId))
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.UNAUTHORIZED_ACCESS_CODE,
                    Message = Const.UNAUTHORIZED_ACCESS_MSG
                });
            }

            var result = await _feedbackService.GetFeedbacksByUserAsync(userId, pageNumber, pageSize);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Get all feedbacks for a specific routine
        /// </summary>
        [HttpGet("routine/{routineId:guid}")]
        public async Task<IActionResult> GetByRoutine(Guid routineId)
        {
            var result = await _feedbackService.GetFeedbacksByRoutineAsync(routineId);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Get all feedbacks for a specific routine step
        /// </summary>
        [HttpGet("step/{stepId:guid}")]
        public async Task<IActionResult> GetByStep(Guid stepId)
        {
            var result = await _feedbackService.GetFeedbacksByStepAsync(stepId);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Get a specific feedback by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _feedbackService.GetFeedbackByIdAsync(id);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Create a new feedback
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FeedbackCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            if (!isAdmin && (!Guid.TryParse(userIdClaim, out var requesterId) || requesterId != dto.UserId))
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.UNAUTHORIZED_ACCESS_CODE,
                    Message = Const.UNAUTHORIZED_ACCESS_MSG
                });
            }

            var result = await _feedbackService.CreateFeedbackAsync(dto);
            var location = result.Data is FeedbackDto created ? $"/api/feedbacks/{created.FeedbackId}" : null;
            return ToHttpResponse(result, location);
        }

        /// <summary>
        /// Update an existing feedback
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FeedbackUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _feedbackService.UpdateFeedbackAsync(id, dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Delete a feedback
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _feedbackService.DeleteFeedbackAsync(id);
            return ToHttpResponse(result);
        }
    }
}
