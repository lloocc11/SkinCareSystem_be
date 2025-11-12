using System;
using SkinCareSystem.Common.Constants;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Common.Utils;
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
                Channel = session.channel,
                State = session.state,
                SpecialistId = session.specialist_id,
                AssignedAt = session.assigned_at.HasValue ? DateTimeHelper.EnsureUtc(session.assigned_at) : null,
                ClosedAt = session.closed_at.HasValue ? DateTimeHelper.EnsureUtc(session.closed_at) : null,
                CreatedAt = DateTimeHelper.EnsureUtc(session.created_at),
                UpdatedAt = session.updated_at.HasValue ? DateTimeHelper.EnsureUtc(session.updated_at) : null
            };
        }

        public static ChatSession ToEntity(this ChatSessionCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var timestamp = DateTimeHelper.UtcNowUnspecified();
            var channel = ChatSessionChannels.TryNormalize(dto.Channel, out var normalized)
                ? normalized
                : ChatSessionChannels.Ai;
            var state = channel == ChatSessionChannels.Specialist
                ? ChatSessionStates.WaitingSpecialist
                : ChatSessionStates.Open;

            return new ChatSession
            {
                session_id = Guid.NewGuid(),
                user_id = dto.UserId,
                title = dto.Title,
                channel = channel,
                state = state,
                specialist_id = null,
                assigned_at = null,
                closed_at = null,
                created_at = timestamp,
                updated_at = timestamp
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

            session.updated_at = DateTimeHelper.UtcNowUnspecified();
        }
    }
}
