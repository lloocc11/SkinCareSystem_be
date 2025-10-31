using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Common.DTOs.UserDTOs;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing user operations.
    /// </summary>
    [Route("api/users")]
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }
        /// <summary>
        /// GET /api/users - Get a paginated list of users (admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetUsersAsync(pageNumber, pageSize);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// GET /api/users/{id} - Get user by ID (admin only)
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// GET /api/users/me - Get current authenticated user's details
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.UNAUTHORIZED_ACCESS_CODE,
                    Message = Const.UNAUTHORIZED_ACCESS_MSG
                });
            }

            var result = await _userService.GetUserByIdAsync(userId);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// POST /api/users - Create a new user (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _userService.CreateUserAsync(dto);
            var location = result.Data is UserDto created ? $"/api/users/{created.UserId}" : null;
            return ToHttpResponse(result, location);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _userService.UpdateUserAsync(id, dto);
            return ToHttpResponse(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var result = await _userService.SoftDeleteUserAsync(id);
            return ToHttpResponse(result);
        }
    }
}
