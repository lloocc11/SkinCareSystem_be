using System;
using SkinCareSystem.Common.DTOs.Rule;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class RuleMapper
    {
        public static RuleDto? ToDto(this Rule rule)
        {
            if (rule == null)
            {
                return null;
            }

            return new RuleDto
            {
                RuleId = rule.rule_id,
                Recommendation = rule.recommendation,
                UrgencyLevel = rule.urgency_level,
                CreatedAt = rule.created_at
            };
        }

        public static Rule ToEntity(this RuleCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Rule
            {
                rule_id = Guid.NewGuid(),
                recommendation = dto.Recommendation,
                urgency_level = dto.UrgencyLevel,
                created_at = DateTime.Now
            };
        }

        public static void ApplyUpdate(this Rule rule, RuleUpdateDto dto)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Recommendation))
            {
                rule.recommendation = dto.Recommendation;
            }

            if (!string.IsNullOrWhiteSpace(dto.UrgencyLevel))
            {
                rule.urgency_level = dto.UrgencyLevel;
            }
        }
    }
}
