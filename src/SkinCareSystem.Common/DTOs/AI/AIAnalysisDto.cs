using System;

namespace SkinCareSystem.Common.DTOs.AI
{
    public class AIAnalysisDto
    {
        public Guid AnalysisId { get; set; }
        public Guid UserId { get; set; }
        public Guid ChatMessageId { get; set; }
        public string RawInput { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
