using System;
using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.AI
{
    public class AIResponseCreateDto
    {
        [Required]
        public Guid QueryId { get; set; }

        [Required]
        public string ResponseText { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ResponseType { get; set; } = "recommendation";
    }
}
