using System;
using SkinCareSystem.Common.DTOs.MedicalDocument.MedicalDocumentAssetDTOs;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class MedicalDocumentAssetMapper
    {
        public static MedicalDocumentAssetDto? ToDto(this MedicalDocumentAsset asset)
        {
            if (asset == null) return null;

            return new MedicalDocumentAssetDto
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
            };
        }

        public static MedicalDocumentAsset ToEntity(this MedicalDocumentAssetCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new MedicalDocumentAsset
            {
                asset_id = Guid.NewGuid(),
                doc_id = dto.DocId,
                file_url = dto.FileUrl,
                public_id = dto.PublicId,
                provider = string.IsNullOrWhiteSpace(dto.Provider) ? "cloudinary" : dto.Provider,
                mime_type = dto.MimeType,
                size_bytes = dto.SizeBytes,
                width = dto.Width,
                height = dto.Height,
                created_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
        }

        public static void ApplyUpdate(this MedicalDocumentAsset entity, MedicalDocumentAssetUpdateDto dto)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.FileUrl))
            {
                entity.file_url = dto.FileUrl;
            }

            if (dto.PublicId != null)
            {
                entity.public_id = dto.PublicId;
            }

            if (dto.Provider != null)
            {
                entity.provider = dto.Provider;
            }

            if (dto.MimeType != null)
            {
                entity.mime_type = dto.MimeType;
            }

            entity.size_bytes = dto.SizeBytes ?? entity.size_bytes;
            entity.width = dto.Width ?? entity.width;
            entity.height = dto.Height ?? entity.height;
        }
    }
}
