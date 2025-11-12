using System;

namespace SkinCareSystem.Common.DTOs.Chat
{
    public class ChatSessionDto
    {
        public Guid SessionId { get; set; }

        public Guid UserId { get; set; }

        public string? Title { get; set; }

        public string Channel { get; set; } = "ai";

        public string State { get; set; } = "open";

        public Guid? SpecialistId { get; set; }

        public DateTime? AssignedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
