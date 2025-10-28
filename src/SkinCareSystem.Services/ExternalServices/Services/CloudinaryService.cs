using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SkinCareSystem.Common.DTOs.Media;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.Options;

namespace SkinCareSystem.Services.ExternalServices.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4"
        };

        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _settings;

        public CloudinaryService(Cloudinary cloudinary, IOptions<CloudinarySettings> options)
        {
            _cloudinary = cloudinary ?? throw new ArgumentNullException(nameof(cloudinary));
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<MediaUploadResultDto> UploadFileAsync(IFormFile file, string? folderName = null, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File cannot be null or empty.", nameof(file));
            }

            using var stream = file.OpenReadStream();
            return await UploadInternalAsync(stream, file.FileName, folderName, cancellationToken).ConfigureAwait(false);
        }

        public async Task<MediaUploadResultDto> UploadFileByByteAsync(byte[] fileBytes, string fileName, string? folderName = null, CancellationToken cancellationToken = default)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new ArgumentException("File cannot be null or empty.", nameof(fileBytes));
            }

            using var stream = new MemoryStream(fileBytes);
            return await UploadInternalAsync(stream, fileName, folderName, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> DeleteFileAsync(string publicId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                throw new ArgumentException("Public ID cannot be null or empty.", nameof(publicId));
            }

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams, cancellationToken).ConfigureAwait(false);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var message = result.Error?.Message ?? "Unknown Cloudinary deletion error.";
                throw new Exception($"Failed to delete file: {message}");
            }

            return result.Result == "ok" ? "File deleted successfully." : "File deletion failed.";
        }

        public async Task<string> GetFileUrlAsync(string publicId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                throw new ArgumentException("Public ID cannot be null or empty.", nameof(publicId));
            }

            var resource = await _cloudinary.GetResourceAsync(new GetResourceParams(publicId), cancellationToken)
                .ConfigureAwait(false);

            if (resource.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var message = resource.Error?.Message ?? "Unknown Cloudinary error.";
                throw new Exception($"Failed to retrieve file: {message}");
            }

            return resource.SecureUrl?.ToString() ?? resource.Url?.ToString() ?? string.Empty;
        }

        private async Task<MediaUploadResultDto> UploadInternalAsync(Stream stream, string fileName, string? folderName, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(fileName);
            if (!AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");
            }

            var resourceType = extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase) ? "video" : "image";
            var folder = DetermineFolder(folderName, extension);
            var publicId = $"{folder}/{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid():N}";

            RawUploadParams uploadParams = new()
            {
                File = new FileDescription(fileName, stream),
                Folder = folder,
                PublicId = publicId,
                ResourceType = resourceType
            };

            RawUploadResult uploadResult = extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)
                ? await _cloudinary.UploadLargeAsync(uploadParams, cancellationToken).ConfigureAwait(false)
                : await _cloudinary.UploadAsync(uploadParams, cancellationToken).ConfigureAwait(false);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var message = uploadResult.Error?.Message ?? "Unknown Cloudinary upload error.";
                throw new Exception($"Failed to upload file: {message}");
            }

            return new MediaUploadResultDto
            {
                Url = uploadResult.Url?.ToString() ?? string.Empty,
                SecureUrl = uploadResult.SecureUrl?.ToString() ?? string.Empty,
                PublicId = uploadResult.PublicId,
                Format = uploadResult.Format,
                Bytes = uploadResult.Bytes,
                Width = uploadResult.Width,
                Height = uploadResult.Height,
                ResourceType = uploadResult.ResourceType,
                CreatedAt = uploadResult.CreatedAt ?? DateTime.UtcNow
            };
        }

        private string DetermineFolder(string? folderName, string extension)
        {
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                return folderName.Trim();
            }

            if (extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrWhiteSpace(_settings.VideoFolder)
                    ? $"{_settings.Folder}/videos"
                    : _settings.VideoFolder!;
            }

            return $"{_settings.Folder}/images";
        }
    }
}
