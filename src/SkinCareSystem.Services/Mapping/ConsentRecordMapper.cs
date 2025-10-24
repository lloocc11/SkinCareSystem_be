using System;
using SkinCareSystem.Common.DTOs.Consent;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class ConsentRecordMapper
    {
        public static ConsentRecordDto? ToDto(this ConsentRecord consent)
        {
            if (consent == null) return null;

            return new ConsentRecordDto
            {
                ConsentId = consent.ConsentId,
                UserId = consent.UserId,
                ConsentType = consent.ConsentType,
                ConsentText = consent.ConsentText,
                Given = consent.Given,
                GivenAt = consent.GivenAt,
                UserFullName = consent.User?.FullName
            };
        }

        public static ConsentRecord ToEntity(this ConsentRecordCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new ConsentRecord
            {
                ConsentId = Guid.NewGuid(),
                UserId = dto.UserId,
                ConsentType = dto.ConsentType,
                ConsentText = dto.ConsentText,
                Given = dto.Given,
                GivenAt = dto.Given ? DateTime.UtcNow : null
            };
        }

        public static void ApplyUpdate(this ConsentRecord consent, ConsentRecordUpdateDto dto)
        {
            if (consent == null) throw new ArgumentNullException(nameof(consent));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.Given.HasValue)
            {
                consent.Given = dto.Given.Value;
                if (dto.Given.Value && !consent.GivenAt.HasValue)
                    consent.GivenAt = DateTime.UtcNow;
            }

            if (dto.GivenAt.HasValue)
                consent.GivenAt = dto.GivenAt.Value;
        }
    }
}
