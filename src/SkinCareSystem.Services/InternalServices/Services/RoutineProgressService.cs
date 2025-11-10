using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class RoutineProgressService : IRoutineProgressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "completed",
            "skipped"
        };

        public RoutineProgressService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> GetRoutineProgressByInstanceAsync(Guid instanceId, Guid requesterId, bool isAdmin)
        {
            var instance = await _unitOfWork.RoutineInstanceRepository.GetByIdAsync(instanceId);
            if (instance == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine instance not found"
                };
            }

            if (!HasAccess(instance, requesterId, isAdmin))
            {
                return ForbiddenResult();
            }

            var progresses = await _unitOfWork.RoutineProgressRepository.GetByInstanceIdAsync(instanceId);
            var progressDtos = progresses.Select(p => p.ToDto()).Where(dto => dto != null).ToList();

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = progressDtos
            };
        }

        public async Task<IServiceResult> GetRoutineProgressByIdAsync(Guid progressId, Guid requesterId, bool isAdmin)
        {
            var progress = await _unitOfWork.RoutineProgressRepository.GetByIdWithDetailsAsync(progressId);
            if (progress == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine progress not found"
                };
            }

            if (!HasAccess(progress.instance, requesterId, isAdmin))
            {
                return ForbiddenResult();
            }

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = progress.ToDto()
            };
        }

        public async Task<IServiceResult> GetRoutineProgressLogAsync(Guid instanceId, Guid requesterId, bool isAdmin)
        {
            var instance = await _unitOfWork.RoutineInstanceRepository.GetByIdAsync(instanceId);
            if (instance == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine instance not found"
                };
            }

            if (!HasAccess(instance, requesterId, isAdmin))
            {
                return ForbiddenResult();
            }

            var progresses = await _unitOfWork.RoutineProgressRepository.GetByInstanceIdAsync(instanceId);
            var progressDtos = progresses
                .Select(p => p.ToLogEntryDto())
                .Where(dto => dto != null)
                .Select(dto => dto!)
                .OrderByDescending(p => p.CompletedAt)
                .ToList();

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = progressDtos
            };
        }

        public async Task<IServiceResult> CreateRoutineProgressAsync(Guid requesterId, bool isAdmin, RoutineProgressCreateDto dto)
        {
            if (dto == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                };
            }

            var normalizedStatus = NormalizeStatus(dto.Status);
            if (!IsStatusAllowed(normalizedStatus))
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = $"Invalid status '{dto.Status}'. Allowed values: completed, skipped."
                };
            }

            var instance = await _unitOfWork.RoutineInstanceRepository.GetByIdAsync(dto.InstanceId);
            if (instance == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine instance not found"
                };
            }

            if (!HasAccess(instance, requesterId, isAdmin))
            {
                return ForbiddenResult();
            }

            var step = await _unitOfWork.RoutineStepRepository.GetByIdAsync(dto.StepId);
            if (step == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine step not found"
                };
            }

            if (step.routine_id != instance.routine_id)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine step does not belong to the selected routine"
                };
            }

            var completedAt = dto.CompletedAt == default ? DateTime.UtcNow : dto.CompletedAt;
            var duplicate = await _unitOfWork.RoutineProgressRepository.GetByInstanceStepAndDateAsync(dto.InstanceId, dto.StepId, DateOnly.FromDateTime(completedAt));

            if (duplicate != null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_DATA_EXISTED_CODE,
                    Message = "Routine progress already recorded for this step on the selected date"
                };
            }

            var entity = dto.ToEntity(completedAt, normalizedStatus);
            await _unitOfWork.RoutineProgressRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = StatusCodes.Status201Created,
                Message = Const.SUCCESS_CREATE_MSG,
                Data = entity.ToDto()
            };
        }

        public async Task<IServiceResult> UpdateRoutineProgressAsync(Guid progressId, Guid requesterId, bool isAdmin, RoutineProgressUpdateDto dto)
        {
            var progress = await _unitOfWork.RoutineProgressRepository.GetByIdAsync(progressId);
            if (progress == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine progress not found"
                };
            }

            var instance = await _unitOfWork.RoutineInstanceRepository.GetByIdAsync(progress.instance_id);
            if (instance == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine instance not found"
                };
            }

            if (!HasAccess(instance, requesterId, isAdmin))
            {
                return ForbiddenResult();
            }

            DateTime? sanitizedCompletedAt = null;
            if (dto.CompletedAt.HasValue)
            {
                sanitizedCompletedAt = dto.CompletedAt.Value;
                var existingProgress = await _unitOfWork.RoutineProgressRepository.GetByInstanceStepAndDateAsync(progress.instance_id, progress.step_id, DateOnly.FromDateTime(sanitizedCompletedAt.Value));
                if (existingProgress != null && existingProgress.progress_id != progress.progress_id)
                {
                    return new ServiceResult
                    {
                        Status = Const.WARNING_DATA_EXISTED_CODE,
                        Message = "Another check-in already exists for this step on the selected date"
                    };
                }
            }

            string? sanitizedStatus = null;
            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                sanitizedStatus = NormalizeStatus(dto.Status);
                if (!IsStatusAllowed(sanitizedStatus))
                {
                    return new ServiceResult
                    {
                        Status = Const.ERROR_VALIDATION_CODE,
                        Message = $"Invalid status '{dto.Status}'. Allowed values: completed, skipped."
                    };
                }
            }

            var updateDto = new RoutineProgressUpdateDto
            {
                CompletedAt = sanitizedCompletedAt,
                PhotoUrl = dto.PhotoUrl,
                Note = dto.Note,
                Status = sanitizedStatus
            };

            progress.ApplyUpdate(updateDto);

            await _unitOfWork.RoutineProgressRepository.UpdateAsync(progress);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_UPDATE_MSG,
                Data = progress.ToDto()
            };
        }

        public async Task<IServiceResult> DeleteRoutineProgressAsync(Guid progressId, Guid requesterId, bool isAdmin)
        {
            var progress = await _unitOfWork.RoutineProgressRepository.GetByIdAsync(progressId);
            if (progress == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine progress not found"
                };
            }

            var instance = await _unitOfWork.RoutineInstanceRepository.GetByIdAsync(progress.instance_id);
            if (instance == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Routine instance not found"
                };
            }

            if (!HasAccess(instance, requesterId, isAdmin))
            {
                return ForbiddenResult();
            }

            await _unitOfWork.RoutineProgressRepository.RemoveAsync(progress);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_DELETE_MSG
            };
        }

        private static bool HasAccess(RoutineInstance instance, Guid requesterId, bool isAdmin)
        {
            if (instance == null)
            {
                return false;
            }

            return isAdmin || instance.user_id == requesterId;
        }

        private static ServiceResult ForbiddenResult()
        {
            return new ServiceResult
            {
                Status = Const.FORBIDDEN_ACCESS_CODE,
                Message = Const.FORBIDDEN_ACCESS_MSG
            };
        }

        private static bool IsStatusAllowed(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            return AllowedStatuses.Contains(status);
        }

        private static string NormalizeStatus(string? status)
        {
            return string.IsNullOrWhiteSpace(status)
                ? "completed"
                : status.Trim().ToLowerInvariant();
        }
    }
}
