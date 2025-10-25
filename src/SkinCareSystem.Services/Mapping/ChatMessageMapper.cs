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
                MessageId = message.MessageId,
                SessionId = message.SessionId,
                UserId = message.UserId,
                Content = message.Content,
                ImageUrl = message.ImageUrl,
                MessageType = message.MessageType,
                Role = message.Role,
                CreatedAt = message.CreatedAt
            };
        }

        public static ChatMessage ToEntity(this ChatMessageCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new ChatMessage
            {
                MessageId = Guid.NewGuid(),
                SessionId = dto.SessionId,
                UserId = dto.UserId,
                Content = dto.Content,
                ImageUrl = dto.ImageUrl,
                MessageType = dto.MessageType,
                Role = dto.Role,
                CreatedAt = DateTime.Now
            };
        }
    }
}
