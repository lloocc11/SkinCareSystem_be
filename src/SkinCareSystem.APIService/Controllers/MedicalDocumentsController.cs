using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing medical documents
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class MedicalDocumentsController : BaseApiController
    {
        private static readonly ServiceResult RagDisabled = new(410, "RAG document ingestion is disabled. Use LLM routine builder endpoints.");

        [HttpGet]
        public IActionResult GetAllDocuments() => ToHttpResponse(RagDisabled);

        [HttpGet("{id}")]
        public IActionResult GetDocumentById(Guid id) => ToHttpResponse(RagDisabled);

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult CreateDocument() => ToHttpResponse(RagDisabled);

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult UpdateDocument(Guid id) => ToHttpResponse(RagDisabled);

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteDocument(Guid id) => ToHttpResponse(RagDisabled);
    }
}
