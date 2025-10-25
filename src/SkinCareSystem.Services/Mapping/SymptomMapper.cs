using System;
using SkinCareSystem.Common.DTOs.Symptom;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class SymptomMapper
    {
        public static SymptomDto? ToDto(this Symptom symptom)
        {
            if (symptom == null)
            {
                return null;
            }

            return new SymptomDto
            {
                SymptomId = symptom.SymptomId,
                Name = symptom.Name,
                Description = symptom.Description,
                ExampleImageUrl = symptom.ExampleImageUrl,
                CreatedAt = symptom.CreatedAt
            };
        }

        public static Symptom ToEntity(this SymptomCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Symptom
            {
                SymptomId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ExampleImageUrl = dto.ExampleImageUrl,
                CreatedAt = DateTime.Now
            };
        }

        public static void ApplyUpdate(this Symptom symptom, SymptomUpdateDto dto)
        {
            if (symptom == null) throw new ArgumentNullException(nameof(symptom));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                symptom.Name = dto.Name;
            }

            if (dto.Description != null)
            {
                symptom.Description = dto.Description;
            }

            if (dto.ExampleImageUrl != null)
            {
                symptom.ExampleImageUrl = dto.ExampleImageUrl;
            }
        }
    }
}
