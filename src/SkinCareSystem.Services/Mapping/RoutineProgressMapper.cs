using System;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Common.Utils;
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
                Status = progress.status,
                IrritationLevel = progress.irritation_level,
                MoodNote = progress.mood_note
            };
        }

        public static RoutineProgress ToEntity(this RoutineProgressCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var completedAt = dto.CompletedAt == default
                ? DateTimeHelper.UtcNowUnspecified()
                : DateTimeHelper.EnsureUnspecified(dto.CompletedAt);
            var status = dto.Status ?? "completed";

            return dto.ToEntity(completedAt, status);
        }

        public static RoutineProgress ToEntity(this RoutineProgressCreateDto dto, DateTime completedAt, string status)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new RoutineProgress
            {
                progress_id = Guid.NewGuid(),
                instance_id = dto.InstanceId,
                step_id = dto.StepId,
                completed_at = DateTimeHelper.EnsureUnspecified(completedAt),
                photo_url = dto.PhotoUrl,
                note = dto.Note,
                status = status,
                irritation_level = dto.IrritationLevel,
                mood_note = dto.MoodNote
            };
        }

        public static void ApplyUpdate(this RoutineProgress progress, RoutineProgressUpdateDto dto)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.CompletedAt.HasValue)
            {
                progress.completed_at = DateTimeHelper.EnsureUnspecified(dto.CompletedAt.Value);
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

            if (dto.IrritationLevel.HasValue)
            {
                progress.irritation_level = dto.IrritationLevel;
            }

            if (dto.MoodNote != null)
            {
                progress.mood_note = dto.MoodNote;
            }
        }

        public static RoutineProgressLogEntryDto? ToLogEntryDto(this RoutineProgress progress)
        {
            if (progress == null)
            {
                return null;
            }

            return new RoutineProgressLogEntryDto
            {
                ProgressId = progress.progress_id,
                InstanceId = progress.instance_id,
                StepId = progress.step_id,
                StepInstruction = progress.step?.instruction ?? string.Empty,
                TimeOfDay = progress.step?.time_of_day ?? string.Empty,
                StepOrder = progress.step?.step_order ?? 0,
                CompletedAt = progress.completed_at,
                Status = progress.status,
                PhotoUrl = progress.photo_url,
                Note = progress.note,
                IrritationLevel = progress.irritation_level,
                MoodNote = progress.mood_note
            };
        }
    }
}
