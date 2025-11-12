using System;
using SkinCareSystem.Common.Constants;

namespace SkinCareSystem.Common.DTOs.Chat
{
    public class ChatSessionCreateDto
    {
        public Guid UserId { get; set; }

        public string? Title { get; set; }

        public string Channel { get; set; } = ChatSessionChannels.Ai;
    }
}
