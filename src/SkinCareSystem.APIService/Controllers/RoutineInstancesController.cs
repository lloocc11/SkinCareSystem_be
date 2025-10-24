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
    [Route("api/routine-instances")]
    [Authorize]
    public class RoutineInstancesController : BaseApiController
    {
        private readonly IRoutineInstanceService _routineInstanceService;

        public RoutineInstancesController(IRoutineInstanceService routineInstanceService)
        {
            _routineInstanceService = routineInstanceService ?? throw new ArgumentNullException(nameof(routineInstanceService));
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
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

            var result = await _routineInstanceService.GetRoutineInstancesByUserAsync(userId, pageNumber, pageSize);
            return ToHttpResponse(result);
        }

        [HttpGet("routine/{routineId:guid}")]
        public async Task<IActionResult> GetByRoutine(Guid routineId)
        {
            var result = await _routineInstanceService.GetRoutineInstancesByRoutineAsync(routineId);
            return ToHttpResponse(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _routineInstanceService.GetRoutineInstanceByIdAsync(id);
            return ToHttpResponse(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoutineInstanceCreateDto dto)
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

            var result = await _routineInstanceService.CreateRoutineInstanceAsync(dto);
            var location = result.Data is RoutineInstanceDto created ? $"/api/routine-instances/{created.InstanceId}" : null;
            return ToHttpResponse(result, location);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RoutineInstanceUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _routineInstanceService.UpdateRoutineInstanceAsync(id, dto);
            return ToHttpResponse(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _routineInstanceService.DeleteRoutineInstanceAsync(id);
            return ToHttpResponse(result);
        }
    }
}
