using System;
using SkinCareSystem.Common.DTOs.Feedback;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class FeedbackMapper
    {
        public static FeedbackDto? ToDto(this Feedback feedback)
        {
            if (feedback == null)
            {
                return null;
            }

            return new FeedbackDto
            {
                FeedbackId = feedback.feedback_id,
                RoutineId = feedback.routine_id,
                StepId = feedback.step_id,
                UserId = feedback.user_id,
                Rating = feedback.rating,
                Comment = feedback.comment,
                CreatedAt = feedback.created_at,
                UserFullName = feedback.user?.full_name
            };
        }

        public static Feedback ToEntity(this FeedbackCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Feedback
            {
                feedback_id = Guid.NewGuid(),
                routine_id = dto.RoutineId,
                step_id = dto.StepId,
                user_id = dto.UserId,
                rating = dto.Rating,
                comment = dto.Comment,
                created_at = DateTimeHelper.UtcNowUnspecified()
            };
        }

        public static void ApplyUpdate(this Feedback feedback, FeedbackUpdateDto dto)
        {
            if (feedback == null) throw new ArgumentNullException(nameof(feedback));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.Rating.HasValue)
            {
                feedback.rating = dto.Rating.Value;
            }

            if (dto.Comment != null)
            {
                feedback.comment = dto.Comment;
            }
        }
    }
}
