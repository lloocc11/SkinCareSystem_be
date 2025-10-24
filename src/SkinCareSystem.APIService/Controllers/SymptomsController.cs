using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.DTOs.Symptom;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing symptoms
    /// </summary>
    [Route("api/symptoms")]
    [Authorize]
    public class SymptomsController : BaseApiController
    {
        private readonly ISymptomService _symptomService;

        public SymptomsController(ISymptomService symptomService)
        {
            _symptomService = symptomService ?? throw new ArgumentNullException(nameof(symptomService));
        }

        /// <summary>
        /// Get all symptoms with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSymptoms([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _symptomService.GetSymptomsAsync(pageNumber, pageSize);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Get a specific symptom by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _symptomService.GetSymptomByIdAsync(id);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Create a new symptom
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] SymptomCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _symptomService.CreateSymptomAsync(dto);
            var location = result.Data is SymptomDto created ? $"/api/symptoms/{created.SymptomId}" : null;
            return ToHttpResponse(result, location);
        }

        /// <summary>
        /// Update an existing symptom
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SymptomUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _symptomService.UpdateSymptomAsync(id, dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Delete a symptom
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _symptomService.DeleteSymptomAsync(id);
            return ToHttpResponse(result);
        }
    }

    /// <summary>
    /// Controller for managing user symptom reports
    /// </summary>
    [Route("api/user-symptoms")]
    [Authorize]
    public class UserSymptomsController : BaseApiController
    {
        private readonly IUserSymptomService _userSymptomService;

        public UserSymptomsController(IUserSymptomService userSymptomService)
        {
            _userSymptomService = userSymptomService ?? throw new ArgumentNullException(nameof(userSymptomService));
        }

        /// <summary>
        /// Get all symptoms reported by a specific user
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            if (!isAdmin && (!Guid.TryParse(userIdClaim, out var requesterId) || requesterId != userId))
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.UNAUTHORIZED_ACCESS_CODE,
                    Message = Const.UNAUTHORIZED_ACCESS_MSG
                });
            }

            var result = await _userSymptomService.GetUserSymptomsByUserAsync(userId);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Get a specific user symptom by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _userSymptomService.GetUserSymptomByIdAsync(id);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Create a new user symptom report
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserSymptomCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            if (!isAdmin && (!Guid.TryParse(userIdClaim, out var requesterId) || requesterId != dto.UserId))
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.UNAUTHORIZED_ACCESS_CODE,
                    Message = Const.UNAUTHORIZED_ACCESS_MSG
                });
            }

            var result = await _userSymptomService.CreateUserSymptomAsync(dto);
            var location = result.Data is UserSymptomDto created ? $"/api/user-symptoms/{created.UserSymptomId}" : null;
            return ToHttpResponse(result, location);
        }

        /// <summary>
        /// Update an existing user symptom report
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserSymptomUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _userSymptomService.UpdateUserSymptomAsync(id, dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// Delete a user symptom report
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userSymptomService.DeleteUserSymptomAsync(id);
            return ToHttpResponse(result);
        }
    }
}
