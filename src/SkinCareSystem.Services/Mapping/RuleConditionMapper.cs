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
                RuleConditionId = ruleCondition.RuleConditionId,
                RuleId = ruleCondition.RuleId,
                SymptomId = ruleCondition.SymptomId,
                QuestionId = ruleCondition.QuestionId,
                Operator = ruleCondition.Operator,
                Value = ruleCondition.Value
            };
        }

        public static RuleCondition ToEntity(this RuleConditionCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new RuleCondition
            {
                RuleConditionId = Guid.NewGuid(),
                RuleId = dto.RuleId,
                SymptomId = dto.SymptomId,
                QuestionId = dto.QuestionId,
                Operator = dto.Operator,
                Value = dto.Value
            };
        }

        public static void ApplyUpdate(this RuleCondition ruleCondition, RuleConditionUpdateDto dto)
        {
            if (ruleCondition == null) throw new ArgumentNullException(nameof(ruleCondition));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.SymptomId.HasValue)
            {
                ruleCondition.SymptomId = dto.SymptomId;
            }

            if (dto.QuestionId.HasValue)
            {
                ruleCondition.QuestionId = dto.QuestionId;
            }

            if (!string.IsNullOrWhiteSpace(dto.Operator))
            {
                ruleCondition.Operator = dto.Operator;
            }

            if (dto.Value != null)
            {
                ruleCondition.Value = dto.Value;
            }
        }
    }
}
