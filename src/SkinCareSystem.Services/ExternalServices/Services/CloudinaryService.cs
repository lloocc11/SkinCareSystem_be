using System;
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
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _settings;

        public CloudinaryService(IOptions<CloudinarySettings> options)
        {
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(_settings.CloudName) ||
                string.IsNullOrWhiteSpace(_settings.ApiKey) ||
                string.IsNullOrWhiteSpace(_settings.ApiSecret))
            {
                throw new InvalidOperationException("Cloudinary settings are missing or incomplete.");
            }

            var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
            _cloudinary = new Cloudinary(account)
            {
                Api = { Secure = true }
            };
        }

        public async Task<MediaUploadResultDto> UploadFileAsync(IFormFile file, string? folderName = null, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is required for upload.", nameof(file));
            }

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = string.IsNullOrWhiteSpace(folderName) ? _settings.Folder : folderName,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken).ConfigureAwait(false);

            if (uploadResult.StatusCode >= System.Net.HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException($"Cloudinary upload failed with status {uploadResult.StatusCode} and message {uploadResult.Error?.Message}");
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
                CreatedAt = uploadResult.CreatedAt
            };
        }

        public async Task<MediaUploadResultDto> UploadFileByByteAsync(byte[] fileBytes, string fileName, string? folderName = null, CancellationToken cancellationToken = default)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new ArgumentException("File bytes are required for upload.", nameof(fileBytes));
            }

            await using var stream = new System.IO.MemoryStream(fileBytes);
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = string.IsNullOrWhiteSpace(folderName) ? _settings.Folder : folderName,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken).ConfigureAwait(false);

            if (uploadResult.StatusCode >= System.Net.HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException($"Cloudinary upload failed with status {uploadResult.StatusCode} and message {uploadResult.Error?.Message}");
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
                CreatedAt = uploadResult.CreatedAt
            };
        }

        public async Task<string> DeleteFileAsync(string publicId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return "error: publicId is required";
            }

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deletionParams).ConfigureAwait(false);
            return result.Result == "ok" ? "ok" : $"error: {result.Error?.Message ?? "unknown error"}";
        }

        public Task<string> GetFileUrlAsync(string publicId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return Task.FromResult(string.Empty);
            }

            var url = _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
            return Task.FromResult(url ?? string.Empty);
        }
    }
}
