using System;
using SkinCareSystem.Common.DTOs.Feedback;
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
                FeedbackId = feedback.FeedbackId,
                RoutineId = feedback.RoutineId,
                StepId = feedback.StepId,
                UserId = feedback.UserId,
                Rating = feedback.Rating,
                Comment = feedback.Comment,
                CreatedAt = feedback.CreatedAt,
                UserFullName = feedback.User?.FullName
            };
        }

        public static Feedback ToEntity(this FeedbackCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Feedback
            {
                FeedbackId = Guid.NewGuid(),
                RoutineId = dto.RoutineId,
                StepId = dto.StepId,
                UserId = dto.UserId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.Now
            };
        }

        public static void ApplyUpdate(this Feedback feedback, FeedbackUpdateDto dto)
        {
            if (feedback == null) throw new ArgumentNullException(nameof(feedback));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.Rating.HasValue)
            {
                feedback.Rating = dto.Rating.Value;
            }

            if (dto.Comment != null)
            {
                feedback.Comment = dto.Comment;
            }
        }
    }
}
