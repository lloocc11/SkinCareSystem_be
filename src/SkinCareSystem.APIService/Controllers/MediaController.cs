using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.APIService.Models;
using SkinCareSystem.Services.ExternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/media")]
    public class MediaController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public MediaController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        }

        /// <summary>
        /// Upload a media file to Cloudinary.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="folder">Optional target folder.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Upload metadata.</returns>
        [HttpPost("upload")]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)] // 100 MB
        [RequestSizeLimit(104857600)]
        public async Task<IActionResult> UploadAsync([FromForm] IFormFile file, [FromForm] string? folder = null, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(Api.Fail("Image file is required.", StatusCodes.Status400BadRequest));
            }

            var result = await _cloudinaryService.UploadFileAsync(file, folder, cancellationToken).ConfigureAwait(false);
            return Ok(Api.Ok(result, "Uploaded successfully."));
        }

        /// <summary>
        /// Delete an asset from Cloudinary.
        /// </summary>
        /// <param name="publicId">Cloudinary public id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Deletion status.</returns>
        [HttpDelete("{publicId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteAsync(string publicId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return BadRequest(Api.Fail("publicId is required.", StatusCodes.Status400BadRequest));
            }

            var message = await _cloudinaryService.DeleteFileAsync(publicId, cancellationToken).ConfigureAwait(false);

            return Ok(Api.Ok(new { publicId, message }, "Deleted successfully."));
        }
    }
}
