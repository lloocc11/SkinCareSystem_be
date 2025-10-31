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
    [Route("api/routine-progress")]
    [Authorize]
    public class RoutineProgressController : BaseApiController
    {
        private readonly IRoutineProgressService _routineProgressService;

        public RoutineProgressController(IRoutineProgressService routineProgressService)
        {
            _routineProgressService = routineProgressService ?? throw new ArgumentNullException(nameof(routineProgressService));
        }

        [HttpGet("instance/{instanceId:guid}")]
        public async Task<IActionResult> GetByInstance(Guid instanceId)
        {
            if (!TryGetRequester(out var requesterId, out var errorResult))
            {
                return errorResult!;
            }

            var isAdmin = User.IsInRole("admin");
            var result = await _routineProgressService.GetRoutineProgressByInstanceAsync(instanceId, requesterId, isAdmin);
            return ToHttpResponse(result);
        }

        [HttpGet("instance/{instanceId:guid}/log")]
        public async Task<IActionResult> GetLog(Guid instanceId)
        {
            if (!TryGetRequester(out var requesterId, out var errorResult))
            {
                return errorResult!;
            }

            var isAdmin = User.IsInRole("admin");
            var result = await _routineProgressService.GetRoutineProgressLogAsync(instanceId, requesterId, isAdmin);
            return ToHttpResponse(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (!TryGetRequester(out var requesterId, out var errorResult))
            {
                return errorResult!;
            }

            var isAdmin = User.IsInRole("admin");
            var result = await _routineProgressService.GetRoutineProgressByIdAsync(id, requesterId, isAdmin);
            return ToHttpResponse(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoutineProgressCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            if (!TryGetRequester(out var requesterId, out var errorResult))
            {
                return errorResult!;
            }

            var isAdmin = User.IsInRole("admin");
            var result = await _routineProgressService.CreateRoutineProgressAsync(requesterId, isAdmin, dto);
            var location = result.Data is RoutineProgressDto created ? $"/api/routine-progress/{created.ProgressId}" : null;
            return ToHttpResponse(result, location);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RoutineProgressUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            if (!TryGetRequester(out var requesterId, out var errorResult))
            {
                return errorResult!;
            }

            var isAdmin = User.IsInRole("admin");
            var result = await _routineProgressService.UpdateRoutineProgressAsync(id, requesterId, isAdmin, dto);
            return ToHttpResponse(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!TryGetRequester(out var requesterId, out var errorResult))
            {
                return errorResult!;
            }

            var isAdmin = User.IsInRole("admin");
            var result = await _routineProgressService.DeleteRoutineProgressAsync(id, requesterId, isAdmin);
            return ToHttpResponse(result);
        }

        private bool TryGetRequester(out Guid requesterId, out IActionResult? errorResult)
        {
            requesterId = Guid.Empty;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out requesterId))
            {
                errorResult = ToHttpResponse(new ServiceResult
                {
                    Status = Const.UNAUTHORIZED_ACCESS_CODE,
                    Message = Const.UNAUTHORIZED_ACCESS_MSG
                });
                return false;
            }

            errorResult = null;
            return true;
        }
    }
}
