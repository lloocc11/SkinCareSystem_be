using System;
using SkinCareSystem.Common.DTOs.AI;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class AIMapper
    {
        public static AIAnalysisDto ToDto(this AIAnalysis entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return new AIAnalysisDto
            {
                AnalysisId = entity.analysis_id,
                UserId = entity.user_id,
                ChatMessageId = entity.chat_message_id,
                RawInput = entity.raw_input,
                Result = entity.result,
                Confidence = entity.confidence,
                CreatedAt = entity.created_at,
                UpdatedAt = entity.updated_at
            };
        }

        public static AIAnalysis ToEntity(this AIAnalysisCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new AIAnalysis
            {
                analysis_id = Guid.NewGuid(),
                user_id = dto.UserId,
                chat_message_id = dto.ChatMessageId,
                raw_input = dto.RawInput,
                result = dto.Result,
                confidence = dto.Confidence,
                created_at = DateTimeHelper.UtcNowUnspecified(),
                updated_at = DateTimeHelper.UtcNowUnspecified()
            };
        }

        public static AIResponseDto ToDto(this AIResponse entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return new AIResponseDto
            {
                ResponseId = entity.response_id,
                QueryId = entity.query_id,
                ResponseText = entity.response_text,
                ResponseType = entity.response_type,
                CreatedAt = entity.created_at
            };
        }

        public static AIResponse ToEntity(this AIResponseCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new AIResponse
            {
                response_id = Guid.NewGuid(),
                query_id = dto.QueryId,
                response_text = dto.ResponseText,
                response_type = dto.ResponseType,
                created_at = DateTimeHelper.UtcNowUnspecified()
            };
        }
    }
}
