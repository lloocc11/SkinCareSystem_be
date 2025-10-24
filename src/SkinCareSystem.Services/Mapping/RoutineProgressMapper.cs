using System;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class RoutineProgressMapper
    {
        public static RoutineProgressDto? ToDto(this RoutineProgress progress)
        {
            if (progress == null)
            {
                return null;
            }

            return new RoutineProgressDto
            {
                ProgressId = progress.ProgressId,
                InstanceId = progress.InstanceId,
                StepId = progress.StepId,
                CompletedAt = progress.CompletedAt,
                PhotoUrl = progress.PhotoUrl,
                Note = progress.Note,
                Status = progress.Status
            };
        }

        public static RoutineProgress ToEntity(this RoutineProgressCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new RoutineProgress
            {
                ProgressId = Guid.NewGuid(),
                InstanceId = dto.InstanceId,
                StepId = dto.StepId,
                CompletedAt = dto.CompletedAt,
                PhotoUrl = dto.PhotoUrl,
                Note = dto.Note,
                Status = dto.Status ?? "completed"
            };
        }

        public static void ApplyUpdate(this RoutineProgress progress, RoutineProgressUpdateDto dto)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.CompletedAt.HasValue)
            {
                progress.CompletedAt = dto.CompletedAt.Value;
            }

            if (dto.PhotoUrl != null)
            {
                progress.PhotoUrl = dto.PhotoUrl;
            }

            if (dto.Note != null)
            {
                progress.Note = dto.Note;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                progress.Status = dto.Status;
            }
        }
    }
}
