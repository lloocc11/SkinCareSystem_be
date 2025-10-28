using System;
using SkinCareSystem.Common.DTOs.Symptom;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class UserSymptomMapper
    {
        public static UserSymptomDto? ToDto(this UserSymptom userSymptom)
        {
            if (userSymptom == null)
            {
                return null;
            }

            return new UserSymptomDto
            {
                UserSymptomId = userSymptom.user_symptom_id,
                UserId = userSymptom.user_id,
                SymptomId = userSymptom.symptom_id,
                ReportedAt = userSymptom.reported_at,
                UserFullName = userSymptom.user?.full_name,
                SymptomName = userSymptom.symptom?.name
            };
        }

        public static UserSymptom ToEntity(this UserSymptomCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new UserSymptom
            {
                user_symptom_id = Guid.NewGuid(),
                user_id = dto.UserId,
                symptom_id = dto.SymptomId,
                reported_at = DateTime.Now
            };
        }

        public static void ApplyUpdate(this UserSymptom userSymptom, UserSymptomUpdateDto dto)
        {
            if (userSymptom == null) throw new ArgumentNullException(nameof(userSymptom));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.ReportedAt.HasValue)
            {
                userSymptom.reported_at = dto.ReportedAt.Value;
            }
        }
    }
}
