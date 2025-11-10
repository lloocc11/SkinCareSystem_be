using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.DTOs.Question;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing questions
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class QuestionsController : BaseApiController
    {
        private readonly IQuestionService _questionService;

        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        /// <summary>
        /// Get all questions
        /// </summary>
        /// <returns>List of all questions</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllQuestions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _questionService.GetQuestionsAsync(pageNumber, pageSize);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Get a specific question by ID
        /// </summary>
        /// <param name="id">Question ID</param>
        /// <returns>Question details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestionById(Guid id)
        {
            var result = await _questionService.GetQuestionByIdAsync(id);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Create a new question (Admin only)
        /// </summary>
        /// <param name="dto">Question creation data</param>
        /// <returns>Created question</returns>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateDto dto)
        {
            var result = await _questionService.CreateQuestionAsync(dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Update an existing question (Admin only)
        /// </summary>
        /// <param name="id">Question ID</param>
        /// <param name="dto">Question update data</param>
        /// <returns>Updated question</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] QuestionUpdateDto dto)
        {
            var result = await _questionService.UpdateQuestionAsync(id, dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Delete a question (Admin only)
        /// </summary>
        /// <param name="id">Question ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            var result = await _questionService.DeleteQuestionAsync(id);
            return ToHttpResponse(result);
        }
    }
}
