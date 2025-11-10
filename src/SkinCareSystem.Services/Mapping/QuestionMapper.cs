using System;
using SkinCareSystem.Common.DTOs.Question;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class QuestionMapper
    {
        public static QuestionDto? ToDto(this Question question)
        {
            if (question == null) return null;

            return new QuestionDto
            {
                QuestionId = question.question_id,
                Text = question.text,
                Type = question.type,
                Options = question.options,
                CreatedAt = question.created_at
            };
        }

        public static Question ToEntity(this QuestionCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Question
            {
                question_id = Guid.NewGuid(),
                text = dto.Text,
                type = dto.Type,
                options = dto.Options,
                created_at = DateTimeHelper.UtcNowUnspecified()
            };
        }

        public static void ApplyUpdate(this Question question, QuestionUpdateDto dto)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Text))
                question.text = dto.Text;
            if (!string.IsNullOrWhiteSpace(dto.Type))
                question.type = dto.Type;
            if (dto.Options != null)
                question.options = dto.Options;
        }
    }
}
