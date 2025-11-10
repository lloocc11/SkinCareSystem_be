using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Common.DTOs.Pagination;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class RoutineService : IRoutineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "draft", "published", "archived"
        };

        private static readonly HashSet<string> AllowedRoutineTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "template", "personalized"
        };

        public RoutineService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> GetRoutinesAsync(int pageNumber, int pageSize)
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
                var query = _unitOfWork.RoutineRepository.GetAllQueryable()
                    .OrderByDescending(r => r.created_at);

                var totalItems = await query.CountAsync();
                if (totalItems == 0)
                {
                    return new ServiceResult
                    {
                        Status = Const.WARNING_NO_DATA_CODE,
                        Message = Const.WARNING_NO_DATA_MSG
                    };
                }

                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var routines = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var routineDtos = routines
                    .Select(r => r.ToDto())
                    .Where(dto => dto != null)
                    .Select(dto => dto!)
                    .ToList();

                var pagedResult = new PagedResult<RoutineDto>
                {
                    Items = routineDtos,
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

        public async Task<IServiceResult> GetRoutinesByUserAsync(Guid userId, int pageNumber, int pageSize)
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
                var totalItems = await _unitOfWork.RoutineRepository.CountByUserIdAsync(userId);
                if (totalItems == 0)
                {
                    return new ServiceResult
                    {
                        Status = Const.WARNING_NO_DATA_CODE,
                        Message = Const.WARNING_NO_DATA_MSG
                    };
                }

                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var routines = await _unitOfWork.RoutineRepository.GetByUserIdAsync(userId, pageNumber, pageSize);

                var routineDtos = routines
                    .Select(r => r.ToDto())
                    .Where(dto => dto != null)
                    .Select(dto => dto!)
                    .ToList();

                var pagedResult = new PagedResult<RoutineDto>
                {
                    Items = routineDtos,
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

        public async Task<IServiceResult> GetRoutineByIdAsync(Guid routineId)
        {
            var routine = await _unitOfWork.RoutineRepository.GetByIdWithDetailsAsync(routineId);
            if (routine == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine not found"
                };
            }

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = routine.ToDto()
            };
        }

        public async Task<IServiceResult> CreateRoutineAsync(RoutineCreateDto dto)
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

            if (dto.AnalysisId.HasValue)
            {
                var analysis = await _unitOfWork.AIAnalysisRepository.GetByIdAsync(dto.AnalysisId.Value);
                if (analysis == null)
                {
                    return new ServiceResult
                    {
                        Status = Const.ERROR_VALIDATION_CODE,
                        Message = "Analysis not found"
                    };
                }
            }

            if (dto.ParentRoutineId.HasValue)
            {
                var parentRoutine = await _unitOfWork.RoutineRepository.GetByIdAsync(dto.ParentRoutineId.Value);
                if (parentRoutine == null)
                {
                    return new ServiceResult
                    {
                        Status = Const.ERROR_VALIDATION_CODE,
                        Message = "Parent routine not found"
                    };
                }
            }

            var normalizedType = NormalizeRoutineType(dto.RoutineType);
            var normalizedStatus = NormalizeRoutineStatus(dto.Status);

            if (!AllowedRoutineTypes.Contains(normalizedType))
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = $"Invalid routine type '{dto.RoutineType}'."
                };
            }

            if (!AllowedStatuses.Contains(normalizedStatus))
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = $"Invalid routine status '{dto.Status}'."
                };
            }

            dto.RoutineType = normalizedType;
            dto.Status = normalizedStatus;
            dto.Version = dto.Version <= 0 ? 1 : dto.Version;

            var entity = dto.ToEntity();
            await _unitOfWork.RoutineRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = StatusCodes.Status201Created,
                Message = Const.SUCCESS_CREATE_MSG,
                Data = entity.ToDto()
            };
        }

        public async Task<IServiceResult> UpdateRoutineAsync(Guid routineId, RoutineUpdateDto dto)
        {
            var routine = await _unitOfWork.RoutineRepository.GetByIdAsync(routineId);
            if (routine == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine not found"
                };
            }

            if (!string.IsNullOrWhiteSpace(dto.RoutineType))
            {
                var normalizedType = NormalizeRoutineType(dto.RoutineType);
                if (!AllowedRoutineTypes.Contains(normalizedType))
                {
                    return new ServiceResult
                    {
                        Status = Const.ERROR_VALIDATION_CODE,
                        Message = $"Invalid routine type '{dto.RoutineType}'."
                    };
                }

                dto.RoutineType = normalizedType;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                var normalizedStatus = NormalizeRoutineStatus(dto.Status);
                if (!AllowedStatuses.Contains(normalizedStatus))
                {
                    return new ServiceResult
                    {
                        Status = Const.ERROR_VALIDATION_CODE,
                        Message = $"Invalid routine status '{dto.Status}'."
                    };
                }

                dto.Status = normalizedStatus;
            }

            routine.ApplyUpdate(dto);
            await _unitOfWork.RoutineRepository.UpdateAsync(routine);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_UPDATE_MSG,
                Data = routine.ToDto()
            };
        }

        public async Task<IServiceResult> DeleteRoutineAsync(Guid routineId)
        {
            var routine = await _unitOfWork.RoutineRepository.GetByIdAsync(routineId);
            if (routine == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Routine not found"
                };
            }

            routine.status = "archived";
            routine.updated_at = DateTimeHelper.UtcNowUnspecified();

            await _unitOfWork.RoutineRepository.UpdateAsync(routine);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_DELETE_MSG,
                Data = routine.ToDto()
            };
        }

        private static string NormalizeRoutineStatus(string? status)
        {
            return string.IsNullOrWhiteSpace(status)
                ? "draft"
                : status.Trim().ToLowerInvariant();
        }

        private static string NormalizeRoutineType(string? type)
        {
            return string.IsNullOrWhiteSpace(type)
                ? "template"
                : type.Trim().ToLowerInvariant();
        }
    }
}
