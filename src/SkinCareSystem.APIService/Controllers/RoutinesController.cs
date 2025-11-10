using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Controller for managing routines.
    /// </summary>
    [Route("api/routines")]
    [Authorize]
    public class RoutinesController : BaseApiController
    {
        private readonly IRoutineService _routineService;

        public RoutinesController(IRoutineService routineService)
        {
            _routineService = routineService ?? throw new ArgumentNullException(nameof(routineService));
        }
        /// <summary>
        /// GET /api/routines - Get all routines (admin only)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetRoutines([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _routineService.GetRoutinesAsync(pageNumber, pageSize);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// GET /api/routines/user/{userId} - Get routines by user ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetRoutinesByUser(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
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

            var result = await _routineService.GetRoutinesByUserAsync(userId, pageNumber, pageSize);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// GET /api/routines/{id} - Get routine by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _routineService.GetRoutineByIdAsync(id);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// POST /api/routines - Create a new routine
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "admin,specialist")]
        public async Task<IActionResult> Create([FromBody] RoutineCreateDto dto)
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

            if (!isAdmin)
            {
                if (!Guid.TryParse(userIdClaim, out var requesterId))
                {
                    return ToHttpResponse(new ServiceResult
                    {
                        Status = Const.UNAUTHORIZED_ACCESS_CODE,
                        Message = Const.UNAUTHORIZED_ACCESS_MSG
                    });
                }

                dto.UserId = requesterId;
            }

            var result = await _routineService.CreateRoutineAsync(dto);
            var location = result.Data is RoutineDto created ? $"/api/routines/{created.RoutineId}" : null;
            return ToHttpResponse(result, location);
        }
        /// <summary>
        /// PUT /api/routines/{id} - Update an existing routine
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "admin,specialist")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RoutineUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _routineService.UpdateRoutineAsync(id, dto);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// DELETE /api/routines/{id} - Delete a routine
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "admin,specialist")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _routineService.DeleteRoutineAsync(id);
            return ToHttpResponse(result);
        }
    }
}
