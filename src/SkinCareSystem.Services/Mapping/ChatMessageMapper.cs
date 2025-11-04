using System;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class ChatMessageMapper
    {
        public static ChatMessageDto ToDto(this ChatMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var createdAt = DateTimeHelper.EnsureUnspecified(message.created_at ?? DateTimeHelper.UtcNowUnspecified());

            return new ChatMessageDto
            {
                MessageId = message.message_id,
                SessionId = message.session_id,
                UserId = message.user_id ?? Guid.Empty,
                Content = message.content,
                ImageUrl = message.image_url,
                MessageType = message.message_type,
                Role = message.role,
                CreatedAt = DateTimeHelper.EnsureUtc(createdAt)
            };
        }

        public static ChatMessage ToEntity(this ChatMessageCreateDto dto, string role, string messageType, DateTime timestamp)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            ArgumentException.ThrowIfNullOrEmpty(role);
            ArgumentException.ThrowIfNullOrEmpty(messageType);

            return new ChatMessage
            {
                message_id = Guid.NewGuid(),
                session_id = dto.SessionId,
                user_id = dto.UserId,
                content = dto.Content,
                image_url = dto.ImageUrl,
                message_type = messageType,
                role = role,
                created_at = timestamp
            };
        }

        public static ChatMessage CreateAssistantMessage(Guid sessionId, string content, string? imageUrl, DateTime timestamp)
        {
            return new ChatMessage
            {
                message_id = Guid.NewGuid(),
                session_id = sessionId,
                user_id = null,
                content = content,
                image_url = imageUrl,
                message_type = string.IsNullOrWhiteSpace(imageUrl) ? "text" : "mixed",
                role = "assistant",
                created_at = timestamp
            };
        }
    }
}
