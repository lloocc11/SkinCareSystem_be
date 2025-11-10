using System;
using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.AI
{
    public class AIAnalysisCreateDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ChatMessageId { get; set; }

        [Required]
        public string RawInput { get; set; } = string.Empty;

        [Required]
        public string Result { get; set; } = string.Empty;

        [Range(0, 1)]
        public double Confidence { get; set; }
    }
}
