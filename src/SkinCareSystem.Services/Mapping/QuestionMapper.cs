using System;
using SkinCareSystem.Common.DTOs.Question;
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
                QuestionId = question.QuestionId,
                Text = question.Text,
                Type = question.Type,
                Options = question.Options,
                CreatedAt = question.CreatedAt
            };
        }

        public static Question ToEntity(this QuestionCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Question
            {
                QuestionId = Guid.NewGuid(),
                Text = dto.Text,
                Type = dto.Type,
                Options = dto.Options,
                CreatedAt = DateTime.Now
            };
        }

        public static void ApplyUpdate(this Question question, QuestionUpdateDto dto)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Text))
                question.Text = dto.Text;
            if (!string.IsNullOrWhiteSpace(dto.Type))
                question.Type = dto.Type;
            if (dto.Options != null)
                question.Options = dto.Options;
        }
    }
}
