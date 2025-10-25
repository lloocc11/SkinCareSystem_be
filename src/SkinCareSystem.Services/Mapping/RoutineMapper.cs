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
                RoutineId = routine.RoutineId,
                UserId = routine.UserId,
                AnalysisId = routine.AnalysisId,
                Description = routine.Description,
                Version = routine.Version,
                ParentRoutineId = routine.ParentRoutineId,
                Status = routine.Status,
                CreatedAt = routine.CreatedAt,
                UpdatedAt = routine.UpdatedAt,
                UserFullName = routine.User?.FullName
            };
        }

        public static Routine ToEntity(this RoutineCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Routine
            {
                RoutineId = Guid.NewGuid(),
                UserId = dto.UserId,
                AnalysisId = dto.AnalysisId,
                Description = dto.Description,
                Version = 1,
                ParentRoutineId = dto.ParentRoutineId,
                Status = dto.Status ?? "active",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        public static void ApplyUpdate(this Routine routine, RoutineUpdateDto dto)
        {
            if (routine == null) throw new ArgumentNullException(nameof(routine));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                routine.Description = dto.Description;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                routine.Status = dto.Status;
            }

            routine.UpdatedAt = DateTime.Now;
        }
    }
}
