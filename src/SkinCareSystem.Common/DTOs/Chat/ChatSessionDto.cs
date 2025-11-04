using System;

namespace SkinCareSystem.Common.DTOs.Chat
{
    public class ChatSessionDto
    {
        public Guid SessionId { get; set; }

        public Guid UserId { get; set; }

        public string? Title { get; set; }

        public string Status { get; set; } = "active";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
