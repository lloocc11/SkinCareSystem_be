using System;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class ChatMessageMapper
    {
        public static ChatMessageDto ToDto(this ChatMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            return new ChatMessageDto
            {
                MessageId = message.message_id,
                SessionId = message.session_id,
                UserId = message.user_id,
                Content = message.content,
                ImageUrl = message.image_url,
                MessageType = message.message_type,
                Role = message.role,
                CreatedAt = message.created_at
            };
        }

        public static ChatMessage ToEntity(this ChatMessageCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new ChatMessage
            {
                message_id = Guid.NewGuid(),
                session_id = dto.SessionId,
                user_id = dto.UserId,
                content = dto.Content,
                image_url = dto.ImageUrl,
                message_type = dto.MessageType,
                role = dto.Role,
                created_at = DateTime.Now
            };
        }
    }
}
