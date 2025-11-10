using System;
using Microsoft.AspNetCore.Http;

namespace SkinCareSystem.Common.DTOs.Chat
{
    public class ChatMessageCreateDto
    {
        public Guid SessionId { get; set; }

        public Guid? UserId { get; set; }

        public string? Content { get; set; }

        public IFormFile? Image { get; set; }

        public string? ImageUrl { get; set; }

        [Obsolete("GenerateRoutine is deprecated. Routine templates are suggested automatically.")]
        public bool GenerateRoutine { get; set; } = false;
    }
}
