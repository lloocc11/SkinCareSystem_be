using System;
using SkinCareSystem.Common.DTOs.Consent;
using SkinCareSystem.Common.Utils;
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
                ConsentId = consent.consent_id,
                UserId = consent.user_id,
                ConsentType = consent.consent_type,
                ConsentText = consent.consent_text,
                Given = consent.given,
                GivenAt = consent.given_at,
                UserFullName = consent.user?.full_name
            };
        }

        public static ConsentRecord ToEntity(this ConsentRecordCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new ConsentRecord
            {
                consent_id = Guid.NewGuid(),
                user_id = dto.UserId,
                consent_type = dto.ConsentType,
                consent_text = dto.ConsentText,
                given = dto.Given,
                given_at = dto.Given ? DateTimeHelper.UtcNowUnspecified() : null
            };
        }

        public static void ApplyUpdate(this ConsentRecord consent, ConsentRecordUpdateDto dto)
        {
            if (consent == null) throw new ArgumentNullException(nameof(consent));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.Given.HasValue)
            {
                consent.given = dto.Given.Value;
                if (dto.Given.Value && !consent.given_at.HasValue)
                    consent.given_at = DateTimeHelper.UtcNowUnspecified();
            }

            if (dto.GivenAt.HasValue)
                consent.given_at = DateTimeHelper.EnsureUnspecified(dto.GivenAt.Value);
        }
    }
}
