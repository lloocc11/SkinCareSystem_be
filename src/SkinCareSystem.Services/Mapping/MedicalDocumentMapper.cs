using System;
using SkinCareSystem.Common.DTOs.MedicalDocument;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class MedicalDocumentMapper
    {
        public static MedicalDocumentDto? ToDto(this MedicalDocument document)
        {
            if (document == null) return null;

            return new MedicalDocumentDto
            {
                DocId = document.DocId,
                Title = document.Title,
                Content = document.Content,
                Source = document.Source,
                Status = document.Status,
                LastUpdated = document.LastUpdated,
                CreatedAt = document.CreatedAt
            };
        }

        public static MedicalDocument ToEntity(this MedicalDocumentCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new MedicalDocument
            {
                DocId = Guid.NewGuid(),
                Title = dto.Title,
                Content = dto.Content,
                Source = dto.Source,
                Status = dto.Status ?? "active",
                LastUpdated = DateTime.Now,
                CreatedAt = DateTime.Now
            };
        }

        public static void ApplyUpdate(this MedicalDocument document, MedicalDocumentUpdateDto dto)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Title))
                document.Title = dto.Title;
            if (!string.IsNullOrWhiteSpace(dto.Content))
                document.Content = dto.Content;
            if (dto.Source != null)
                document.Source = dto.Source;
            if (!string.IsNullOrWhiteSpace(dto.Status))
                document.Status = dto.Status;
            
            document.LastUpdated = DateTime.Now;
        }
    }
}
