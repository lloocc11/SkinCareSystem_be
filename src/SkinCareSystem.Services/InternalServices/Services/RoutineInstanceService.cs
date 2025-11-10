using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SkinCareSystem.Common.DTOs.Pagination;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class RoutineInstanceService : IRoutineInstanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "planned",
            "active",
            "paused",
            "completed",
            "cancelled"
        };

        public RoutineInstanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> GetRoutineInstancesByUserAsync(Guid userId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                };
            }

            try
            {
                var totalItems = await _unitOfWork.RoutineInstanceRepository.CountByUserIdAsync(userId);
                if (totalItems == 0)
                {
                    return new ServiceResult
                    {
                        Status = Const.WARNING_NO_DATA_CODE,
                        Message = Const.WARNING_NO_DATA_MSG
                    };
                }

                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var instances = await _unitOfWork.RoutineInstanceRepository.GetByUserIdAsync(userId, pageNumber, pageSize);

                var instanceDtos = instances
                    .Select(i => i.ToDto())
                    .Where(dto => dto != null)
                    .Select(dto => dto!)
                    .ToList();

                var pagedResult = new PagedResult<RoutineInstanceDto>
                {
                    Items = instanceDtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                return new ServiceResult
                {
                    Status = Const.SUCCESS_READ_CODE,
                    Message = Const.SUCCESS_READ_MSG,
                    Data = pagedResult
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_EXCEPTION,
                    Message = ex.Message
                };
            }
        }

        public async Task<IServiceResult> GetRoutineInstancesByRoutineAsync(Guid routineId)
        {
            var routine = await _unitOfWork.RoutineRepository.GetByIdAsync(routineId);
            if (routine == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine not found"
                };
            }

            var instances = await _unitOfWork.RoutineInstanceRepository.GetByRoutineIdAsync(routineId);
            var instanceDtos = instances.Select(i => i.ToDto()).Where(dto => dto != null).ToList();

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = instanceDtos
            };
        }

        public async Task<IServiceResult> GetRoutineInstanceByIdAsync(Guid instanceId)
        {
            var instance = await _unitOfWork.RoutineInstanceRepository.GetByIdWithDetailsAsync(instanceId);
            if (instance == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine instance not found"
                };
            }

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = instance.ToDto()
            };
        }

        public async Task<IServiceResult> CreateRoutineInstanceAsync(RoutineInstanceCreateDto dto)
        {
            if (dto == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                };
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "User not found"
                };
            }

            var routine = await _unitOfWork.RoutineRepository.GetByIdAsync(dto.RoutineId);
            if (routine == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine not found"
                };
            }
            if (!string.Equals(routine.routine_type, "template", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(routine.status, "published", StringComparison.OrdinalIgnoreCase))
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine must be a published template to create an instance"
                };
            }

            var normalizedStatus = NormalizeStatus(dto.Status ?? "planned");

            if (!AllowedStatuses.Contains(normalizedStatus))
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = $"Invalid routine instance status '{dto.Status}'."
                };
            }

            var startDate = dto.StartDate == default
                ? DateOnly.FromDateTime(DateTime.UtcNow)
                : dto.StartDate;

            if (dto.EndDate.HasValue && dto.EndDate.Value < startDate)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "End date cannot be before start date"
                };
            }

            var existingInstance = await _unitOfWork.RoutineInstanceRepository.GetActiveInstanceAsync(dto.UserId, dto.RoutineId);
            if (existingInstance != null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_DATA_EXISTED_CODE,
                    Message = "An active routine instance already exists for this routine"
                };
            }

            dto.StartDate = startDate;
            dto.Status = normalizedStatus;

            var entity = dto.ToEntity();
            entity.adherence_score = null;
            await _unitOfWork.RoutineInstanceRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = StatusCodes.Status201Created,
                Message = Const.SUCCESS_CREATE_MSG,
                Data = entity.ToDto()
            };
        }

        public async Task<IServiceResult> UpdateRoutineInstanceAsync(Guid instanceId, RoutineInstanceUpdateDto dto)
        {
            if (dto.AdherenceScore.HasValue && (dto.AdherenceScore < 0 || dto.AdherenceScore > 100))
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Adherence score must be between 0 and 100."
                };
            }

            var instance = await _unitOfWork.RoutineInstanceRepository.GetByIdAsync(instanceId);
            if (instance == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine instance not found"
                };
            }

            if (dto.EndDate.HasValue && dto.EndDate.Value < instance.start_date)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "End date cannot be before start date"
                };
            }

            string? sanitizedStatus = null;
            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                sanitizedStatus = NormalizeStatus(dto.Status);
                if (!AllowedStatuses.Contains(sanitizedStatus))
                {
                    return new ServiceResult
                    {
                        Status = Const.ERROR_VALIDATION_CODE,
                        Message = $"Invalid routine instance status '{dto.Status}'."
                    };
                }
            }

            var updateDto = new RoutineInstanceUpdateDto
            {
                EndDate = dto.EndDate,
                Status = sanitizedStatus
            };

            if (dto.AdherenceScore.HasValue)
            {
                instance.adherence_score = dto.AdherenceScore.Value;
            }

            instance.ApplyUpdate(updateDto);
            await _unitOfWork.RoutineInstanceRepository.UpdateAsync(instance);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_UPDATE_MSG,
                Data = instance.ToDto()
            };
        }

        public async Task<IServiceResult> DeleteRoutineInstanceAsync(Guid instanceId)
        {
            var instance = await _unitOfWork.RoutineInstanceRepository.GetByIdAsync(instanceId);
            if (instance == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine instance not found"
                };
            }

            instance.status = "cancelled";
            instance.end_date ??= DateOnly.FromDateTime(DateTime.UtcNow);
            await _unitOfWork.RoutineInstanceRepository.UpdateAsync(instance);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_DELETE_MSG,
                Data = instance.ToDto()
            };
        }

        public async Task<IServiceResult> UpdateRoutineInstanceStatusAsync(Guid instanceId, string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Status is required."
                };
            }

            var instance = await _unitOfWork.RoutineInstanceRepository.GetByIdAsync(instanceId);
            if (instance == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine instance not found"
                };
            }

            var currentStatus = NormalizeStatus(instance.status);
            var nextStatus = NormalizeStatus(status);

            if (!AllowedStatuses.Contains(nextStatus))
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = $"Invalid routine instance status '{status}'."
                };
            }

            if (!IsValidTransition(currentStatus, nextStatus))
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = $"Không thể chuyển trạng thái từ '{currentStatus}' sang '{nextStatus}'."
                };
            }

            instance.status = nextStatus;
            if (nextStatus is "cancelled" or "completed")
            {
                instance.end_date ??= DateOnly.FromDateTime(DateTime.UtcNow);
            }

            await _unitOfWork.RoutineInstanceRepository.UpdateAsync(instance);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_UPDATE_MSG,
                Data = instance.ToDto()
            };
        }

        public async Task<IServiceResult> RecalculateAdherenceScoreAsync(Guid instanceId)
        {
            var instance = await _unitOfWork.RoutineInstanceRepository.GetByIdAsync(instanceId);
            if (instance == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine instance not found"
                };
            }

            var progresses = await _unitOfWork.RoutineProgressRepository.GetByInstanceIdAsync(instanceId);
            ApplyAdherenceMetrics(instance, progresses);

            await _unitOfWork.RoutineInstanceRepository.UpdateAsync(instance);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_UPDATE_MSG,
                Data = instance.ToDto()
            };
        }

        private static string NormalizeStatus(string status)
        {
            return string.IsNullOrWhiteSpace(status)
                ? "planned"
                : status.Trim().ToLowerInvariant();
        }

        private static bool IsValidTransition(string current, string next)
        {
            if (string.Equals(current, next, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return (current, next) switch
            {
                ("planned", "active") => true,
                ("planned", "cancelled") => true,
                ("active", "paused") => true,
                ("active", "completed") => true,
                ("active", "cancelled") => true,
                ("paused", "active") => true,
                ("paused", "cancelled") => true,
                _ => false
            };
        }

        private static void ApplyAdherenceMetrics(RoutineInstance instance, IReadOnlyList<RoutineProgress> progressLogs)
        {
            if (progressLogs == null || progressLogs.Count == 0)
            {
                instance.adherence_score = null;
                return;
            }

            var completed = progressLogs.Count(p => string.Equals(p.status, "completed", StringComparison.OrdinalIgnoreCase));
            var missed = progressLogs.Count(p => string.Equals(p.status, "missed", StringComparison.OrdinalIgnoreCase));
            var denominator = completed + missed;

            instance.adherence_score = denominator == 0
                ? null
                : Math.Round((decimal)completed / denominator * 100m, 2);

            if (progressLogs.Count > 0 && string.Equals(instance.status, "planned", StringComparison.OrdinalIgnoreCase))
            {
                instance.status = "active";
            }

            if (progressLogs.Count > 0 && progressLogs.All(p => string.Equals(p.status, "completed", StringComparison.OrdinalIgnoreCase)))
            {
                instance.status = "completed";
                instance.end_date ??= DateOnly.FromDateTime(DateTime.UtcNow);
            }
        }
    }
}
