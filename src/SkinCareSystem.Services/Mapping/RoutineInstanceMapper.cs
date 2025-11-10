using System;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class RoutineInstanceMapper
    {
        public static RoutineInstanceDto? ToDto(this RoutineInstance instance)
        {
            if (instance == null)
            {
                return null;
            }

            return new RoutineInstanceDto
            {
                InstanceId = instance.instance_id,
                RoutineId = instance.routine_id,
                UserId = instance.user_id,
                StartDate = instance.start_date,
                EndDate = instance.end_date,
                Status = instance.status,
                CreatedAt = instance.created_at,
                AdherenceScore = instance.adherence_score,
                UserFullName = instance.user?.full_name
            };
        }

        public static RoutineInstance ToEntity(this RoutineInstanceCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new RoutineInstance
            {
                instance_id = Guid.NewGuid(),
                routine_id = dto.RoutineId,
                user_id = dto.UserId,
                start_date = dto.StartDate,
                end_date = dto.EndDate,
                status = dto.Status ?? "planned",
                created_at = DateTimeHelper.UtcNowUnspecified(),
                adherence_score = null
            };
        }

        public static void ApplyUpdate(this RoutineInstance instance, RoutineInstanceUpdateDto dto)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.EndDate.HasValue)
            {
                instance.end_date = dto.EndDate.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                instance.status = dto.Status;
            }

            if (dto.AdherenceScore.HasValue)
            {
                instance.adherence_score = dto.AdherenceScore;
            }
        }
    }
}
