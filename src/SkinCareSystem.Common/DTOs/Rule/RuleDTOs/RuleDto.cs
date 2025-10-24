using System;

namespace SkinCareSystem.Common.DTOs.Rule
{
    public class RuleDto
    {
        public Guid RuleId { get; set; }
        public string Recommendation { get; set; } = null!;
        public string UrgencyLevel { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }
}
