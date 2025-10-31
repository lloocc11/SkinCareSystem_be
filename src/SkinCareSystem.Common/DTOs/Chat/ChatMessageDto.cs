using System;

namespace SkinCareSystem.Common.DTOs.Chat
{
    public class ChatMessageDto
    {
        public Guid MessageId { get; set; }
        public Guid SessionId { get; set; }
        public Guid? UserId { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public string MessageType { get; set; } = "text";
        public string Role { get; set; } = "user";
        public DateTime? CreatedAt { get; set; }
    }
}
