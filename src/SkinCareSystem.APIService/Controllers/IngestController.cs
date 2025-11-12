using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.APIService.Controllers;

[Route("api/ingest")]
[Authorize(Roles = "admin,specialist")]
public class IngestController : BaseApiController
{
    private static readonly ServiceResult RagDisabled = new(410, "RAG document ingestion is disabled. Use LLM routine builder endpoints.");

    /// <summary>
    /// Upload medical documents and enqueue them for embedding.
    /// </summary>
    [HttpPost("documents")]
    public IActionResult IngestDocuments() => ToHttpResponse(RagDisabled);

    /// <summary>
    /// Chunk & embed a document's textual content.
    /// </summary>
    [HttpPost("documents/{documentId:guid}/embed")]
    public IActionResult EmbedDocument(Guid documentId) => ToHttpResponse(RagDisabled);
}
