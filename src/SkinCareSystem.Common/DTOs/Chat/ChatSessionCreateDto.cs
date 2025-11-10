using System;

namespace SkinCareSystem.Common.DTOs.Chat
{
    public class ChatSessionCreateDto
    {
        public Guid UserId { get; set; }

        public string? Title { get; set; }
    }
}
