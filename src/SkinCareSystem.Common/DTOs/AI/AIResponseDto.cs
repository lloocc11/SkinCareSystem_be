using System;

namespace SkinCareSystem.Common.DTOs.AI
{
    public class AIResponseDto
    {
        public Guid ResponseId { get; set; }
        public Guid QueryId { get; set; }
        public string ResponseText { get; set; } = string.Empty;
        public string ResponseType { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
