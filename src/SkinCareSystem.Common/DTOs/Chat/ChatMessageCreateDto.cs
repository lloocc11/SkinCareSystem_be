using System;
using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.Chat
{
    public class ChatMessageCreateDto
    {
        [Required]
        public Guid SessionId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [StringLength(4000)]
        public string? Content { get; set; }

        [Required]
        [StringLength(20)]
        public string MessageType { get; set; } = "text";

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "user";

        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }
}
