using System;
using SkinCareSystem.Common.DTOs.Routine;
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
                InstanceId = instance.InstanceId,
                RoutineId = instance.RoutineId,
                UserId = instance.UserId,
                StartDate = instance.StartDate,
                EndDate = instance.EndDate,
                Status = instance.Status,
                CreatedAt = instance.CreatedAt,
                UserFullName = instance.User?.FullName
            };
        }

        public static RoutineInstance ToEntity(this RoutineInstanceCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new RoutineInstance
            {
                InstanceId = Guid.NewGuid(),
                RoutineId = dto.RoutineId,
                UserId = dto.UserId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status ?? "active",
                CreatedAt = DateTime.UtcNow
            };
        }

        public static void ApplyUpdate(this RoutineInstance instance, RoutineInstanceUpdateDto dto)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.EndDate.HasValue)
            {
                instance.EndDate = dto.EndDate.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                instance.Status = dto.Status;
            }
        }
    }
}
