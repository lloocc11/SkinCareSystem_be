using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.DTOs.MedicalDocument;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing medical documents
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class MedicalDocumentsController : BaseApiController
    {
        private readonly IMedicalDocumentService _medicalDocumentService;

        public MedicalDocumentsController(IMedicalDocumentService medicalDocumentService)
        {
            _medicalDocumentService = medicalDocumentService;
        }

        /// <summary>
        /// Get all medical documents with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of medical documents</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllDocuments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _medicalDocumentService.GetMedicalDocumentsAsync(pageNumber, pageSize);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Get a specific medical document by ID
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>Medical document details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentById(Guid id)
        {
            var result = await _medicalDocumentService.GetMedicalDocumentByIdAsync(id);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Create a new medical document (Admin only)
        /// </summary>
        /// <param name="dto">Document creation data</param>
        /// <returns>Created medical document</returns>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateDocument([FromBody] MedicalDocumentCreateDto dto)
        {
            var result = await _medicalDocumentService.CreateMedicalDocumentAsync(dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Update an existing medical document (Admin only)
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="dto">Document update data</param>
        /// <returns>Updated medical document</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateDocument(Guid id, [FromBody] MedicalDocumentUpdateDto dto)
        {
            var result = await _medicalDocumentService.UpdateMedicalDocumentAsync(id, dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Delete a medical document (Admin only)
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            var result = await _medicalDocumentService.DeleteMedicalDocumentAsync(id);
            return ToHttpResponse(result);
        }
    }
}
