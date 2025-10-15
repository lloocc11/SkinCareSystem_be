using System;
using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.Chat
{
    public class ChatSessionCreateDto
    {
        [Required]
        public Guid UserId { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }
    }
}
