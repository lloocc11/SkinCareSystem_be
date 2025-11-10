using System;
using SkinCareSystem.Common.DTOs.Routine;
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
                version = 1,
                parent_routine_id = dto.ParentRoutineId,
                status = dto.Status ?? "active",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
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

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                routine.status = dto.Status;
            }

            routine.updated_at = DateTime.Now;
        }
    }
}
