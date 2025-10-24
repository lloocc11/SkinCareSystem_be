using System;
using SkinCareSystem.Common.DTOs.Routine;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Mapping
{
    public static class RoutineStepMapper
    {
        public static RoutineStepDto? ToDto(this RoutineStep step)
        {
            if (step == null)
            {
                return null;
            }

            return new RoutineStepDto
            {
                StepId = step.StepId,
                RoutineId = step.RoutineId,
                StepOrder = step.StepOrder,
                Instruction = step.Instruction,
                TimeOfDay = step.TimeOfDay,
                Frequency = step.Frequency
            };
        }

        public static RoutineStep ToEntity(this RoutineStepCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new RoutineStep
            {
                StepId = Guid.NewGuid(),
                RoutineId = dto.RoutineId,
                StepOrder = dto.StepOrder,
                Instruction = dto.Instruction,
                TimeOfDay = dto.TimeOfDay,
                Frequency = dto.Frequency
            };
        }

        public static void ApplyUpdate(this RoutineStep step, RoutineStepUpdateDto dto)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.StepOrder.HasValue)
            {
                step.StepOrder = dto.StepOrder.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Instruction))
            {
                step.Instruction = dto.Instruction;
            }

            if (!string.IsNullOrWhiteSpace(dto.TimeOfDay))
            {
                step.TimeOfDay = dto.TimeOfDay;
            }

            if (!string.IsNullOrWhiteSpace(dto.Frequency))
            {
                step.Frequency = dto.Frequency;
            }
        }
    }
}
