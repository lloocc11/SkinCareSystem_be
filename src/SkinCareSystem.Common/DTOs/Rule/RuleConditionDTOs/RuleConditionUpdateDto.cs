using System;

namespace SkinCareSystem.Common.DTOs.Rule
{
    public class RuleConditionUpdateDto
    {
        public Guid? SymptomId { get; set; }
        public Guid? QuestionId { get; set; }
        public string? Operator { get; set; }
        public string? Value { get; set; }
    }
}
