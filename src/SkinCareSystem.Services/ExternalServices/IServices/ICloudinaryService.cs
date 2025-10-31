using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SkinCareSystem.Common.DTOs.Media;

namespace SkinCareSystem.Services.ExternalServices.IServices
{
    public interface ICloudinaryService
    {
        Task<MediaUploadResultDto> UploadFileAsync(IFormFile file, string? folderName = null, CancellationToken cancellationToken = default);
        Task<MediaUploadResultDto> UploadFileByByteAsync(byte[] fileBytes, string fileName, string? folderName = null, CancellationToken cancellationToken = default);
        Task<string> DeleteFileAsync(string publicId, CancellationToken cancellationToken = default);
        Task<string> GetFileUrlAsync(string publicId, CancellationToken cancellationToken = default);
    }
}
