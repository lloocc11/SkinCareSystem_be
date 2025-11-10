using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.Chat
{
    public class ChatSessionUpdateDto
    {
        [StringLength(200)]
        public string? Title { get; set; }
    }
}
