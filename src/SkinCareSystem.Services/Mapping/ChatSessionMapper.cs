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
                SessionId = session.SessionId,
                UserId = session.UserId,
                Title = session.Title,
                Status = session.Status ?? "active",
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt
            };
        }

        public static ChatSession ToEntity(this ChatSessionCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new ChatSession
            {
                SessionId = Guid.NewGuid(),
                UserId = dto.UserId,
                Title = dto.Title,
                Status = "active",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        public static void ApplyUpdate(this ChatSession session, ChatSessionUpdateDto dto)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                session.Title = dto.Title;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                session.Status = dto.Status;
            }

            session.UpdatedAt = DateTime.Now;
        }
    }
}
