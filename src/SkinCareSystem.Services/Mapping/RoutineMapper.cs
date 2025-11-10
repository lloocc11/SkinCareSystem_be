using System;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class RoutineMapper
    {
        public static RoutineDto? ToDto(this Routine routine)
        {
            if (routine == null)
            {
                return null;
            }

            return new RoutineDto
            {
                RoutineId = routine.routine_id,
                UserId = routine.user_id,
                AnalysisId = routine.analysis_id,
                Description = routine.description,
                Version = routine.version,
                ParentRoutineId = routine.parent_routine_id,
                RoutineType = routine.routine_type,
                TargetSkinType = routine.target_skin_type,
                TargetConditions = routine.target_conditions,
                Status = routine.status,
                CreatedAt = routine.created_at,
                UpdatedAt = routine.updated_at,
                UserFullName = routine.user?.full_name
            };
        }

        public static Routine ToEntity(this RoutineCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Routine
            {
                routine_id = Guid.NewGuid(),
                user_id = dto.UserId,
                analysis_id = dto.AnalysisId,
                description = dto.Description,
                target_skin_type = dto.TargetSkinType,
                target_conditions = dto.TargetConditions,
                routine_type = string.IsNullOrWhiteSpace(dto.RoutineType) ? "template" : dto.RoutineType.Trim().ToLowerInvariant(),
                version = dto.Version <= 0 ? 1 : dto.Version,
                parent_routine_id = dto.ParentRoutineId,
                status = string.IsNullOrWhiteSpace(dto.Status) ? "draft" : dto.Status.Trim().ToLowerInvariant(),
                created_at = DateTimeHelper.UtcNowUnspecified(),
                updated_at = DateTimeHelper.UtcNowUnspecified()
            };
        }

        public static void ApplyUpdate(this Routine routine, RoutineUpdateDto dto)
        {
            if (routine == null) throw new ArgumentNullException(nameof(routine));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                routine.description = dto.Description;
            }

            if (!string.IsNullOrWhiteSpace(dto.TargetSkinType))
            {
                routine.target_skin_type = dto.TargetSkinType;
            }

            if (!string.IsNullOrWhiteSpace(dto.TargetConditions))
            {
                routine.target_conditions = dto.TargetConditions;
            }

            if (!string.IsNullOrWhiteSpace(dto.RoutineType))
            {
                routine.routine_type = dto.RoutineType.Trim().ToLowerInvariant();
            }

            if (dto.Version.HasValue && dto.Version.Value > 0)
            {
                routine.version = dto.Version.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                routine.status = dto.Status.Trim().ToLowerInvariant();
            }

            routine.updated_at = DateTimeHelper.UtcNowUnspecified();
        }
    }
}
