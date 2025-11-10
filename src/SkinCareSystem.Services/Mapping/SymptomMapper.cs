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
                SymptomId = symptom.symptom_id,
                Name = symptom.name,
                Description = symptom.description,
                ExampleImageUrl = symptom.example_image_url,
                CreatedAt = symptom.created_at
            };
        }

        public static Symptom ToEntity(this SymptomCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Symptom
            {
                symptom_id = Guid.NewGuid(),
                name = dto.Name,
                description = dto.Description,
                example_image_url = dto.ExampleImageUrl,
                created_at = DateTime.Now
            };
        }

        public static void ApplyUpdate(this Symptom symptom, SymptomUpdateDto dto)
        {
            if (symptom == null) throw new ArgumentNullException(nameof(symptom));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                symptom.name = dto.Name;
            }

            if (dto.Description != null)
            {
                symptom.description = dto.Description;
            }

            if (dto.ExampleImageUrl != null)
            {
                symptom.example_image_url = dto.ExampleImageUrl;
            }
        }
    }
}
