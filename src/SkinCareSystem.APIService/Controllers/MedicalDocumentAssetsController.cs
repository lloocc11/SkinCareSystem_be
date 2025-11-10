using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.APIService.Models;
using SkinCareSystem.Common.DTOs.MedicalDocument.MedicalDocumentAssetDTOs;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MedicalDocumentAssetsController : BaseApiController
    {
        private readonly IMedicalDocumentAssetService _assetService;
        private readonly ICloudinaryService _cloudinaryService;

        public MedicalDocumentAssetsController(
            IMedicalDocumentAssetService assetService,
            ICloudinaryService cloudinaryService)
        {
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        }

        /// <summary>
        /// Lấy chi tiết ảnh của tài liệu.
        /// </summary>
        [HttpGet("{assetId:guid}")]
        public async Task<IActionResult> GetAssetById(Guid assetId)
        {
            var result = await _assetService.GetAssetByIdAsync(assetId);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// GET /api/documents/{documentId}/assets - Lấy danh sách ảnh theo tài liệu.
        /// </summary>
        [HttpGet("~/api/documents/{documentId:guid}/assets")]
        public async Task<IActionResult> GetAssetsByDocument(Guid documentId)
        {
            var result = await _assetService.GetAssetsByDocumentAsync(documentId);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Tạo bản ghi ảnh (khi đã có URL).
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAsset([FromBody] MedicalDocumentAssetCreateDto dto, CancellationToken cancellationToken)
        {
            var result = await _assetService.CreateAssetAsync(dto, cancellationToken);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// POST /api/documents/{documentId}/assets - Upload ảnh lên Cloudinary và lưu metadata.
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPost("~/api/documents/{documentId:guid}/assets")]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<IActionResult> UploadAsset(Guid documentId, IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(Api.Fail("Image file is required.", StatusCodes.Status400BadRequest));
            }

            var upload = await _cloudinaryService.UploadFileAsync(file, $"medical-documents/{documentId}", cancellationToken).ConfigureAwait(false);

            var createDto = new MedicalDocumentAssetCreateDto
            {
                DocId = documentId,
                FileUrl = string.IsNullOrWhiteSpace(upload.SecureUrl) ? upload.Url : upload.SecureUrl,
                PublicId = upload.PublicId,
                Provider = "cloudinary",
                MimeType = upload.Format,
                SizeBytes = (int?)upload.Bytes,
                Width = upload.Width,
                Height = upload.Height
            };

            var result = await _assetService.CreateAssetAsync(createDto, cancellationToken);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Cập nhật metadata ảnh.
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPut("{assetId:guid}")]
        public async Task<IActionResult> UpdateAsset(Guid assetId, [FromBody] MedicalDocumentAssetUpdateDto dto, CancellationToken cancellationToken)
        {
            var result = await _assetService.UpdateAssetAsync(assetId, dto, cancellationToken);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Xóa ảnh và optional Cloudinary asset.
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpDelete("{assetId:guid}")]
        public async Task<IActionResult> DeleteAsset(Guid assetId, [FromQuery] string? publicId = null, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(publicId))
            {
                await _cloudinaryService.DeleteFileAsync(publicId, cancellationToken).ConfigureAwait(false);
            }

            var result = await _assetService.DeleteAssetAsync(assetId, cancellationToken);
            return ToHttpResponse(result);
        }
    }
}
