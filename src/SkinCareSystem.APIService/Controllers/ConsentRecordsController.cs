using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.DTOs.Consent;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing user consent records
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class ConsentRecordsController : BaseApiController
    {
        private readonly IConsentRecordService _consentRecordService;

        public ConsentRecordsController(IConsentRecordService consentRecordService)
        {
            _consentRecordService = consentRecordService;
        }

        /// <summary>
        /// Get a specific consent record by ID
        /// </summary>
        /// <param name="id">Consent record ID</param>
        /// <returns>Consent record details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetConsentRecordById(Guid id)
        {
            var result = await _consentRecordService.GetConsentRecordByIdAsync(id);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Get consent records for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's consent records</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetConsentRecordsByUserId(Guid userId)
        {
            var result = await _consentRecordService.GetConsentRecordsByUserAsync(userId);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Create a new consent record
        /// </summary>
        /// <param name="dto">Consent record creation data</param>
        /// <returns>Created consent record</returns>
        [HttpPost]
        public async Task<IActionResult> CreateConsentRecord([FromBody] ConsentRecordCreateDto dto)
        {
            var result = await _consentRecordService.CreateConsentRecordAsync(dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Update an existing consent record
        /// </summary>
        /// <param name="id">Consent record ID</param>
        /// <param name="dto">Consent record update data</param>
        /// <returns>Updated consent record</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConsentRecord(Guid id, [FromBody] ConsentRecordUpdateDto dto)
        {
            var result = await _consentRecordService.UpdateConsentRecordAsync(id, dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Delete a consent record (Admin only)
        /// </summary>
        /// <param name="id">Consent record ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConsentRecord(Guid id)
        {
            var result = await _consentRecordService.DeleteConsentRecordAsync(id);
            return ToHttpResponse(result);
        }
    }
}
