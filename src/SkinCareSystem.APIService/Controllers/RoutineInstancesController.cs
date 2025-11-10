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
        /// <summary>
        /// GET /api/routine-instances/user/{userId} - Get routine instances by user ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var isAdmin = User.IsInRole("admin");

            if (!isAdmin)
            {
                if (!TryGetRequester(out var requesterId, out var errorResult))
                {
                    return errorResult!;
                }

                if (requesterId != userId)
                {
                    return ToHttpResponse(new ServiceResult
                    {
                        Status = Const.UNAUTHORIZED_ACCESS_CODE,
                        Message = Const.UNAUTHORIZED_ACCESS_MSG
                    });
                }
            }

            var result = await _routineInstanceService.GetRoutineInstancesByUserAsync(userId, pageNumber, pageSize);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// GET /api/routine-instances/routine/{routineId} - Get routine instances by routine ID
        /// </summary>
        [HttpGet("routine/{routineId:guid}")]
        public async Task<IActionResult> GetByRoutine(Guid routineId)
        {
            var result = await _routineInstanceService.GetRoutineInstancesByRoutineAsync(routineId);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// GET /api/routine-instances/{id} - Get routine instance by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var isAdmin = User.IsInRole("admin");
            Guid requesterId = Guid.Empty;

            if (!isAdmin && !TryGetRequester(out requesterId, out var errorResult))
            {
                return errorResult!;
            }

            var result = await _routineInstanceService.GetRoutineInstanceByIdAsync(id);

            if (!isAdmin && result.Status == Const.SUCCESS_READ_CODE && result.Data is RoutineInstanceDto dto && dto.UserId != requesterId)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.FORBIDDEN_ACCESS_CODE,
                    Message = Const.FORBIDDEN_ACCESS_MSG
                });
            }

            return ToHttpResponse(result);
        }
        /// <summary>
        /// POST /api/routine-instances - Create a new routine instance
        /// </summary>
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

            var isAdmin = User.IsInRole("admin");

            if (!isAdmin)
            {
                if (!TryGetRequester(out var requesterId, out var errorResult))
                {
                    return errorResult!;
                }

                if (requesterId != dto.UserId)
                {
                    return ToHttpResponse(new ServiceResult
                    {
                        Status = Const.UNAUTHORIZED_ACCESS_CODE,
                        Message = Const.UNAUTHORIZED_ACCESS_MSG
                    });
                }
            }

            var result = await _routineInstanceService.CreateRoutineInstanceAsync(dto);
            var location = result.Data is RoutineInstanceDto created ? $"/api/routine-instances/{created.InstanceId}" : null;
            return ToHttpResponse(result, location);
        }
        /// <summary>
        /// POST /api/routine-instances/routine/{routineId}/instances - Create a new routine instance for the authenticated user
        /// </summary>
        [HttpPost("routine/{routineId:guid}/instances")]
        public async Task<IActionResult> CreateForRoutine(Guid routineId, [FromBody] RoutineInstanceStartRequestDto? dto)
        {
            if (!TryGetRequester(out var requesterId, out var errorResult))
            {
                return errorResult!;
            }

            var createDto = new RoutineInstanceCreateDto
            {
                RoutineId = routineId,
                UserId = requesterId,
                StartDate = dto?.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = dto?.EndDate,
                Status = "planned"
            };

            var result = await _routineInstanceService.CreateRoutineInstanceAsync(createDto);
            var location = result.Data is RoutineInstanceDto created ? $"/api/routine-instances/{created.InstanceId}" : null;
            return ToHttpResponse(result, location);
        }
        /// <summary>
        /// PUT /api/routine-instances/{id} - Update an existing routine instance
        /// </summary>
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

            var isAdmin = User.IsInRole("admin");
            if (!isAdmin)
            {
                if (!TryGetRequester(out var requesterId, out var errorResult))
                {
                    return errorResult!;
                }

                var existingResult = await _routineInstanceService.GetRoutineInstanceByIdAsync(id);
                if (existingResult.Status != Const.SUCCESS_READ_CODE)
                {
                    return ToHttpResponse(existingResult);
                }

                if (existingResult.Data is RoutineInstanceDto dtoData && dtoData.UserId != requesterId)
                {
                    return ToHttpResponse(new ServiceResult
                    {
                        Status = Const.FORBIDDEN_ACCESS_CODE,
                        Message = Const.FORBIDDEN_ACCESS_MSG
                    });
                }
            }

            var result = await _routineInstanceService.UpdateRoutineInstanceAsync(id, dto);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// PATCH /api/routine-instances/{id}/status - Update status with allowed transitions.
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] RoutineInstanceStatusUpdateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var isAdmin = User.IsInRole("admin");
            if (!isAdmin)
            {
                if (!TryGetRequester(out var requesterId, out var errorResult))
                {
                    return errorResult!;
                }

                var existingResult = await _routineInstanceService.GetRoutineInstanceByIdAsync(id);
                if (existingResult.Status != Const.SUCCESS_READ_CODE)
                {
                    return ToHttpResponse(existingResult);
                }

                if (existingResult.Data is RoutineInstanceDto dtoData && dtoData.UserId != requesterId)
                {
                    return ToHttpResponse(new ServiceResult
                    {
                        Status = Const.FORBIDDEN_ACCESS_CODE,
                        Message = Const.FORBIDDEN_ACCESS_MSG
                    });
                }
            }

            var result = await _routineInstanceService.UpdateRoutineInstanceStatusAsync(id, dto.Status);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// POST /api/routine-instances/{id}/recalculate-adherence - recompute adherence score.
        /// </summary>
        [HttpPost("{id:guid}/recalculate-adherence")]
        public async Task<IActionResult> RecalculateAdherence(Guid id)
        {
            var isAdmin = User.IsInRole("admin");
            if (!isAdmin)
            {
                if (!TryGetRequester(out var requesterId, out var errorResult))
                {
                    return errorResult!;
                }

                var instanceResult = await _routineInstanceService.GetRoutineInstanceByIdAsync(id);
                if (instanceResult.Status != Const.SUCCESS_READ_CODE)
                {
                    return ToHttpResponse(instanceResult);
                }

                if (instanceResult.Data is RoutineInstanceDto dto && dto.UserId != requesterId)
                {
                    return ToHttpResponse(new ServiceResult
                    {
                        Status = Const.FORBIDDEN_ACCESS_CODE,
                        Message = Const.FORBIDDEN_ACCESS_MSG
                    });
                }
            }

            var result = await _routineInstanceService.RecalculateAdherenceScoreAsync(id);
            return ToHttpResponse(result);
        }
        /// <summary>
        /// DELETE /api/routine-instances/{id} - Delete a routine instance
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var isAdmin = User.IsInRole("admin");
            if (!isAdmin)
            {
                if (!TryGetRequester(out var requesterId, out var errorResult))
                {
                    return errorResult!;
                }

                var existingResult = await _routineInstanceService.GetRoutineInstanceByIdAsync(id);
                if (existingResult.Status != Const.SUCCESS_READ_CODE)
                {
                    return ToHttpResponse(existingResult);
                }

                if (existingResult.Data is RoutineInstanceDto dtoData && dtoData.UserId != requesterId)
                {
                    return ToHttpResponse(new ServiceResult
                    {
                        Status = Const.FORBIDDEN_ACCESS_CODE,
                        Message = Const.FORBIDDEN_ACCESS_MSG
                    });
                }
            }

            var result = await _routineInstanceService.DeleteRoutineInstanceAsync(id);
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
