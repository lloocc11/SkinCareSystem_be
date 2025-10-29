using System;
using SkinCareSystem.Common.DTOs.Rule;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class RuleConditionMapper
    {
        public static RuleConditionDto? ToDto(this RuleCondition ruleCondition)
        {
            if (ruleCondition == null)
            {
                return null;
            }

            return new RuleConditionDto
            {
                RuleConditionId = ruleCondition.rule_condition_id,
                RuleId = ruleCondition.rule_id,
                SymptomId = ruleCondition.symptom_id,
                QuestionId = ruleCondition.question_id,
                Operator = ruleCondition._operator,
                Value = ruleCondition.value
            };
        }

        public static RuleCondition ToEntity(this RuleConditionCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new RuleCondition
            {
                rule_condition_id = Guid.NewGuid(),
                rule_id = dto.RuleId,
                symptom_id = dto.SymptomId,
                question_id = dto.QuestionId,
                _operator = dto.Operator,
                value = dto.Value
            };
        }

        public static void ApplyUpdate(this RuleCondition ruleCondition, RuleConditionUpdateDto dto)
        {
            if (ruleCondition == null) throw new ArgumentNullException(nameof(ruleCondition));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.SymptomId.HasValue)
            {
                ruleCondition.symptom_id = dto.SymptomId;
            }

            if (dto.QuestionId.HasValue)
            {
                ruleCondition.question_id = dto.QuestionId;
            }

            if (!string.IsNullOrWhiteSpace(dto.Operator))
            {
                ruleCondition._operator = dto.Operator;
            }

            if (dto.Value != null)
            {
                ruleCondition.value = dto.Value;
            }
        }
    }
}
