using System;
using System.Collections.Generic;
using System.Linq;
using SkinCareSystem.Common.DTOs.MedicalDocument;
using SkinCareSystem.Common.DTOs.MedicalDocument.MedicalDocumentAssetDTOs;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class MedicalDocumentMapper
    {
        public static MedicalDocumentDto? ToDto(this MedicalDocument document)
        {
            if (document == null) return null;

            var assetDtos = document.MedicalDocumentAssets?
                .Select(asset => new MedicalDocumentAssetDto
                {
                    AssetId = asset.asset_id,
                    DocId = asset.doc_id,
                    FileUrl = asset.file_url,
                    PublicId = asset.public_id,
                    Provider = asset.provider,
                    MimeType = asset.mime_type,
                    SizeBytes = asset.size_bytes,
                    Width = asset.width,
                    Height = asset.height,
                    CreatedAt = asset.created_at
                })
                .ToList() ?? new List<MedicalDocumentAssetDto>();

            return new MedicalDocumentDto
            {
                DocId = document.doc_id,
                Title = document.title,
                Content = document.content,
                Source = document.source,
                Status = document.status,
                LastUpdated = document.last_updated,
                CreatedAt = document.created_at,
                Assets = assetDtos
            };
        }

        public static MedicalDocument ToEntity(this MedicalDocumentCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new MedicalDocument
            {
                doc_id = Guid.NewGuid(),
                title = dto.Title,
                content = dto.Content,
                source = dto.Source,
                status = dto.Status ?? "active",
                last_updated = DateTime.Now,
                created_at = DateTime.Now
            };
        }

        public static void ApplyUpdate(this MedicalDocument document, MedicalDocumentUpdateDto dto)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Title))
                document.title = dto.Title;
            if (!string.IsNullOrWhiteSpace(dto.Content))
                document.content = dto.Content;
            if (dto.Source != null)
                document.source = dto.Source;
            if (!string.IsNullOrWhiteSpace(dto.Status))
                document.status = dto.Status;

            document.last_updated = DateTime.Now;
        }
    }
}
