using System;
using SkinCareSystem.Common.DTOs.AI;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class AIMapper
    {
        public static AIAnalysisDto ToDto(this Aianalysis entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return new AIAnalysisDto
            {
                AnalysisId = entity.AnalysisId,
                UserId = entity.UserId,
                ChatMessageId = entity.ChatMessageId,
                RawInput = entity.RawInput,
                Result = entity.Result,
                Confidence = entity.Confidence,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static Aianalysis ToEntity(this AIAnalysisCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Aianalysis
            {
                AnalysisId = Guid.NewGuid(),
                UserId = dto.UserId,
                ChatMessageId = dto.ChatMessageId,
                RawInput = dto.RawInput,
                Result = dto.Result,
                Confidence = dto.Confidence,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static AIResponseDto ToDto(this Airesponse entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return new AIResponseDto
            {
                ResponseId = entity.ResponseId,
                QueryId = entity.QueryId,
                ResponseText = entity.ResponseText,
                ResponseType = entity.ResponseType,
                CreatedAt = entity.CreatedAt
            };
        }

        public static Airesponse ToEntity(this AIResponseCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Airesponse
            {
                ResponseId = Guid.NewGuid(),
                QueryId = dto.QueryId,
                ResponseText = dto.ResponseText,
                ResponseType = dto.ResponseType,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
