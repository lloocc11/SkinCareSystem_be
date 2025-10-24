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
    [Route("api/routine-steps")]
    [Authorize]
    public class RoutineStepsController : BaseApiController
    {
        private readonly IRoutineStepService _routineStepService;

        public RoutineStepsController(IRoutineStepService routineStepService)
        {
            _routineStepService = routineStepService ?? throw new ArgumentNullException(nameof(routineStepService));
        }

        [HttpGet("routine/{routineId:guid}")]
        public async Task<IActionResult> GetByRoutine(Guid routineId)
        {
            var result = await _routineStepService.GetRoutineStepsByRoutineAsync(routineId);
            return ToHttpResponse(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _routineStepService.GetRoutineStepByIdAsync(id);
            return ToHttpResponse(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoutineStepCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _routineStepService.CreateRoutineStepAsync(dto);
            var location = result.Data is RoutineStepDto created ? $"/api/routine-steps/{created.StepId}" : null;
            return ToHttpResponse(result, location);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RoutineStepUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _routineStepService.UpdateRoutineStepAsync(id, dto);
            return ToHttpResponse(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _routineStepService.DeleteRoutineStepAsync(id);
            return ToHttpResponse(result);
        }
    }
}
