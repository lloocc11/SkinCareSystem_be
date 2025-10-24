using System;
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
            var result = await _routineProgressService.GetRoutineProgressByInstanceAsync(instanceId);
            return ToHttpResponse(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _routineProgressService.GetRoutineProgressByIdAsync(id);
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

            var result = await _routineProgressService.CreateRoutineProgressAsync(dto);
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

            var result = await _routineProgressService.UpdateRoutineProgressAsync(id, dto);
            return ToHttpResponse(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _routineProgressService.DeleteRoutineProgressAsync(id);
            return ToHttpResponse(result);
        }
    }
}
