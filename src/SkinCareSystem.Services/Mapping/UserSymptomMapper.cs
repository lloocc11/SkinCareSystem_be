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
                UserSymptomId = userSymptom.UserSymptomId,
                UserId = userSymptom.UserId,
                SymptomId = userSymptom.SymptomId,
                ReportedAt = userSymptom.ReportedAt,
                UserFullName = userSymptom.User?.FullName,
                SymptomName = userSymptom.Symptom?.Name
            };
        }

        public static UserSymptom ToEntity(this UserSymptomCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new UserSymptom
            {
                UserSymptomId = Guid.NewGuid(),
                UserId = dto.UserId,
                SymptomId = dto.SymptomId,
                ReportedAt = DateTime.Now
            };
        }

        public static void ApplyUpdate(this UserSymptom userSymptom, UserSymptomUpdateDto dto)
        {
            if (userSymptom == null) throw new ArgumentNullException(nameof(userSymptom));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.ReportedAt.HasValue)
            {
                userSymptom.ReportedAt = dto.ReportedAt.Value;
            }
        }
    }
}
