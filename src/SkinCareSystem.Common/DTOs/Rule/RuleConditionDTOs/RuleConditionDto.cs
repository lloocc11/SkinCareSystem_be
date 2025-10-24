using System;

namespace SkinCareSystem.Common.DTOs.Rule
{
    public class RuleConditionDto
    {
        public Guid RuleConditionId { get; set; }
        public Guid RuleId { get; set; }
        public Guid? SymptomId { get; set; }
        public Guid? QuestionId { get; set; }
        public string Operator { get; set; } = null!;
        public string? Value { get; set; }
    }
}
