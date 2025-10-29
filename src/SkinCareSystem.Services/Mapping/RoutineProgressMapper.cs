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
                ProgressId = progress.progress_id,
                InstanceId = progress.instance_id,
                StepId = progress.step_id,
                CompletedAt = progress.completed_at,
                PhotoUrl = progress.photo_url,
                Note = progress.note,
                Status = progress.status
            };
        }

        public static RoutineProgress ToEntity(this RoutineProgressCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new RoutineProgress
            {
                progress_id = Guid.NewGuid(),
                instance_id = dto.InstanceId,
                step_id = dto.StepId,
                completed_at = dto.CompletedAt,
                photo_url = dto.PhotoUrl,
                note = dto.Note,
                status = dto.Status ?? "completed"
            };
        }

        public static void ApplyUpdate(this RoutineProgress progress, RoutineProgressUpdateDto dto)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.CompletedAt.HasValue)
            {
                progress.completed_at = dto.CompletedAt.Value;
            }

            if (dto.PhotoUrl != null)
            {
                progress.photo_url = dto.PhotoUrl;
            }

            if (dto.Note != null)
            {
                progress.note = dto.Note;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                progress.status = dto.Status;
            }
        }
    }
}
