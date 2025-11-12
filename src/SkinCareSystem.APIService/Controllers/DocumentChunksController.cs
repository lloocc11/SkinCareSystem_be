using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing document chunks
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class DocumentChunksController : BaseApiController
    {
        private static readonly ServiceResult RagDisabled = new(410, "RAG document ingestion is disabled. Use LLM routine builder endpoints.");

        [HttpGet("{id}")]
        public IActionResult GetChunkById(Guid id) => ToHttpResponse(RagDisabled);

        [HttpGet("document/{docId}")]
        public IActionResult GetChunksByDocumentId(Guid docId) => ToHttpResponse(RagDisabled);

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult CreateChunk() => ToHttpResponse(RagDisabled);

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult UpdateChunk(Guid id) => ToHttpResponse(RagDisabled);

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteChunk(Guid id) => ToHttpResponse(RagDisabled);
    }
}
