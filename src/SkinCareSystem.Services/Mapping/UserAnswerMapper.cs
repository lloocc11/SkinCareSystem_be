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
                AnswerId = userAnswer.answer_id,
                UserId = userAnswer.user_id,
                QuestionId = userAnswer.question_id,
                AnswerValue = userAnswer.answer_value,
                CreatedAt = userAnswer.created_at,
                UpdatedAt = userAnswer.updated_at,
                UserFullName = userAnswer.user?.full_name,
                QuestionText = userAnswer.question?.text
            };
        }

        public static UserAnswer ToEntity(this UserAnswerCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new UserAnswer
            {
                answer_id = Guid.NewGuid(),
                user_id = dto.UserId,
                question_id = dto.QuestionId,
                answer_value = dto.AnswerValue,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };
        }

        public static void ApplyUpdate(this UserAnswer userAnswer, UserAnswerUpdateDto dto)
        {
            if (userAnswer == null) throw new ArgumentNullException(nameof(userAnswer));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.AnswerValue))
                userAnswer.answer_value = dto.AnswerValue;
            
            userAnswer.updated_at = DateTime.Now;
        }
    }
}
