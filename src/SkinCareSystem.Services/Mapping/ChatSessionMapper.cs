using System;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class ChatSessionMapper
    {
        public static ChatSessionDto ToDto(this ChatSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            return new ChatSessionDto
            {
                SessionId = session.session_id,
                UserId = session.user_id,
                Title = session.title,
                CreatedAt = session.created_at,
                UpdatedAt = session.updated_at
            };
        }

        public static ChatSession ToEntity(this ChatSessionCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new ChatSession
            {
                session_id = Guid.NewGuid(),
                user_id = dto.UserId,
                title = dto.Title,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };
        }

        public static void ApplyUpdate(this ChatSession session, ChatSessionUpdateDto dto)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                session.title = dto.Title;
            }

            session.updated_at = DateTime.Now;
        }
    }
}
