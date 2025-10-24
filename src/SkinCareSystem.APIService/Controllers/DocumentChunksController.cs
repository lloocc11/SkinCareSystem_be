using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.DTOs.MedicalDocument;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing document chunks
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class DocumentChunksController : BaseApiController
    {
        private readonly IDocumentChunkService _documentChunkService;

        public DocumentChunksController(IDocumentChunkService documentChunkService)
        {
            _documentChunkService = documentChunkService;
        }

        /// <summary>
        /// Get a specific document chunk by ID
        /// </summary>
        /// <param name="id">Chunk ID</param>
        /// <returns>Document chunk details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChunkById(Guid id)
        {
            var result = await _documentChunkService.GetDocumentChunkByIdAsync(id);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Get all chunks for a specific document
        /// </summary>
        /// <param name="docId">Document ID</param>
        /// <returns>List of chunks for the document</returns>
        [HttpGet("document/{docId}")]
        public async Task<IActionResult> GetChunksByDocumentId(Guid docId)
        {
            var result = await _documentChunkService.GetDocumentChunksByDocumentAsync(docId);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Create a new document chunk (Admin only)
        /// </summary>
        /// <param name="dto">Chunk creation data</param>
        /// <returns>Created document chunk</returns>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateChunk([FromBody] DocumentChunkCreateDto dto)
        {
            var result = await _documentChunkService.CreateDocumentChunkAsync(dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Update an existing document chunk (Admin only)
        /// </summary>
        /// <param name="id">Chunk ID</param>
        /// <param name="dto">Chunk update data</param>
        /// <returns>Updated document chunk</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateChunk(Guid id, [FromBody] DocumentChunkUpdateDto dto)
        {
            var result = await _documentChunkService.UpdateDocumentChunkAsync(id, dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Delete a document chunk (Admin only)
        /// </summary>
        /// <param name="id">Chunk ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteChunk(Guid id)
        {
            var result = await _documentChunkService.DeleteDocumentChunkAsync(id);
            return ToHttpResponse(result);
        }
    }
}
