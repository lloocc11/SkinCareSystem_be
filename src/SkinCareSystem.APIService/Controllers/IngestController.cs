using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkinCareSystem.Common.DTOs.MedicalDocument;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers;

[Route("api/ingest")]
[Authorize(Roles = "admin,specialist")]
public class IngestController : BaseApiController
{
    private readonly IDocumentIngestService _documentIngestService;
    private readonly ILogger<IngestController> _logger;

    public IngestController(IDocumentIngestService documentIngestService, ILogger<IngestController> logger)
    {
        _documentIngestService = documentIngestService ?? throw new ArgumentNullException(nameof(documentIngestService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Upload medical documents and enqueue them for embedding.
    /// </summary>
    [HttpPost("documents")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104_857_600)] // 100 MB
    public async Task<IActionResult> IngestDocuments([FromForm] IngestDocumentRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE, Const.ERROR_INVALID_DATA_MSG));
        }

        var result = await _documentIngestService.IngestAsync(request).ConfigureAwait(false);
        return ToHttpResponse(result);
    }

    /// <summary>
    /// Chunk & embed a document's textual content.
    /// </summary>
    [HttpPost("documents/{documentId:guid}/embed")]
    public async Task<IActionResult> EmbedDocument(Guid documentId, [FromBody] EmbedDocumentRequestDto request)
    {
        if (documentId == Guid.Empty)
        {
            return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE, "DocumentId is required."));
        }

        var result = await _documentIngestService.EmbedAsync(documentId, request).ConfigureAwait(false);
        return ToHttpResponse(result);
    }
}
