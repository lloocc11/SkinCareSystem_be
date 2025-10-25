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
                RuleId = rule.RuleId,
                Recommendation = rule.Recommendation,
                UrgencyLevel = rule.UrgencyLevel,
                CreatedAt = rule.CreatedAt
            };
        }

        public static Rule ToEntity(this RuleCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Rule
            {
                RuleId = Guid.NewGuid(),
                Recommendation = dto.Recommendation,
                UrgencyLevel = dto.UrgencyLevel,
                CreatedAt = DateTime.Now
            };
        }

        public static void ApplyUpdate(this Rule rule, RuleUpdateDto dto)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Recommendation))
            {
                rule.Recommendation = dto.Recommendation;
            }

            if (!string.IsNullOrWhiteSpace(dto.UrgencyLevel))
            {
                rule.UrgencyLevel = dto.UrgencyLevel;
            }
        }
    }
}
