using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.APIService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MedicalDocumentAssetsController : BaseApiController
    {
        private static readonly ServiceResult RagDisabled = new(410, "RAG document ingestion is disabled. Use LLM routine builder endpoints.");

        [HttpGet("{assetId:guid}")]
        public IActionResult GetAssetById(Guid assetId) => ToHttpResponse(RagDisabled);

        [HttpGet("~/api/documents/{documentId:guid}/assets")]
        public IActionResult GetAssetsByDocument(Guid documentId) => ToHttpResponse(RagDisabled);

        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult CreateAsset() => ToHttpResponse(RagDisabled);

        [Authorize(Roles = "admin")]
        [HttpPost("~/api/documents/{documentId:guid}/assets")]
        public IActionResult UploadAsset(Guid documentId) => ToHttpResponse(RagDisabled);

        [Authorize(Roles = "admin")]
        [HttpPut("{assetId:guid}")]
        public IActionResult UpdateAsset(Guid assetId) => ToHttpResponse(RagDisabled);

        [Authorize(Roles = "admin")]
        [HttpDelete("{assetId:guid}")]
        public IActionResult DeleteAsset(Guid assetId, [FromQuery] string? publicId = null) => ToHttpResponse(RagDisabled);
    }
}
