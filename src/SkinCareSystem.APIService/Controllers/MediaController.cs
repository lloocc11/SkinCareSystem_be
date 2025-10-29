using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.ExternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Quản lý upload/xóa media trên Cloudinary (tự dùng cho tài liệu & chat image).
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/media")]
    public class MediaController : BaseApiController
    {
        private readonly ICloudinaryService _cloudinaryService;

        public MediaController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        }

        /// <summary>
        /// Upload file lên Cloudinary.
        /// </summary>
        [HttpPost("upload")]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)] // 100 MB
        [RequestSizeLimit(104857600)]
        public async Task<IActionResult> UploadAsync( IFormFile file, string? folder = null, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                var invalid = new ServiceResult(Const.ERROR_INVALID_DATA_CODE, "Image file is required.");
                return ToHttpResponse(invalid);
            }

            var result = await _cloudinaryService.UploadFileAsync(file, folder, cancellationToken).ConfigureAwait(false);
            var success = new ServiceResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, result);
            return ToHttpResponse(success);
        }

        /// <summary>
        /// Xóa file khỏi Cloudinary.
        /// </summary>
        [HttpDelete("{publicId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteAsync(string publicId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                var invalid = new ServiceResult(Const.ERROR_INVALID_DATA_CODE, "publicId is required.");
                return ToHttpResponse(invalid);
            }

            var message = await _cloudinaryService.DeleteFileAsync(publicId, cancellationToken).ConfigureAwait(false);
            var success = new ServiceResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG, new { publicId, message });
            return ToHttpResponse(success);
        }
    }
}
