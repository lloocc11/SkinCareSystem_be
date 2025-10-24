using System;
using SkinCareSystem.Common.DTOs.Question;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class UserAnswerMapper
    {
        public static UserAnswerDto? ToDto(this UserAnswer userAnswer)
        {
            if (userAnswer == null) return null;

            return new UserAnswerDto
            {
                AnswerId = userAnswer.AnswerId,
                UserId = userAnswer.UserId,
                QuestionId = userAnswer.QuestionId,
                AnswerValue = userAnswer.AnswerValue,
                CreatedAt = userAnswer.CreatedAt,
                UpdatedAt = userAnswer.UpdatedAt,
                UserFullName = userAnswer.User?.FullName,
                QuestionText = userAnswer.Question?.Text
            };
        }

        public static UserAnswer ToEntity(this UserAnswerCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new UserAnswer
            {
                AnswerId = Guid.NewGuid(),
                UserId = dto.UserId,
                QuestionId = dto.QuestionId,
                AnswerValue = dto.AnswerValue,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static void ApplyUpdate(this UserAnswer userAnswer, UserAnswerUpdateDto dto)
        {
            if (userAnswer == null) throw new ArgumentNullException(nameof(userAnswer));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.AnswerValue))
                userAnswer.AnswerValue = dto.AnswerValue;
            
            userAnswer.UpdatedAt = DateTime.UtcNow;
        }
    }
}
