using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.DTOs.Question;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing user answers to questions
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class UserAnswersController : BaseApiController
    {
        private readonly IUserAnswerService _userAnswerService;

        public UserAnswersController(IUserAnswerService userAnswerService)
        {
            _userAnswerService = userAnswerService;
        }

        /// <summary>
        /// Get a specific user answer by ID
        /// </summary>
        /// <param name="id">Answer ID</param>
        /// <returns>User answer details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnswerById(Guid id)
        {
            var result = await _userAnswerService.GetUserAnswerByIdAsync(id);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Get all answers by a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's answers</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAnswersByUserId(Guid userId)
        {
            var result = await _userAnswerService.GetUserAnswersByUserAsync(userId);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Create a new user answer
        /// </summary>
        /// <param name="dto">Answer creation data</param>
        /// <returns>Created answer</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAnswer([FromBody] UserAnswerCreateDto dto)
        {
            var result = await _userAnswerService.CreateUserAnswerAsync(dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Update an existing user answer
        /// </summary>
        /// <param name="id">Answer ID</param>
        /// <param name="dto">Answer update data</param>
        /// <returns>Updated answer</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnswer(Guid id, [FromBody] UserAnswerUpdateDto dto)
        {
            var result = await _userAnswerService.UpdateUserAnswerAsync(id, dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Delete a user answer
        /// </summary>
        /// <param name="id">Answer ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnswer(Guid id)
        {
            var result = await _userAnswerService.DeleteUserAnswerAsync(id);
            return ToHttpResponse(result);
        }
    }
}
