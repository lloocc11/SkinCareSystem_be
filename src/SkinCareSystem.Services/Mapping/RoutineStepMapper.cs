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
                StepId = step.step_id,
                RoutineId = step.routine_id,
                StepOrder = step.step_order,
                Instruction = step.instruction,
                TimeOfDay = step.time_of_day,
                Frequency = step.frequency
            };
        }

        public static RoutineStep ToEntity(this RoutineStepCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new RoutineStep
            {
                step_id = Guid.NewGuid(),
                routine_id = dto.RoutineId,
                step_order = dto.StepOrder,
                instruction = dto.Instruction,
                time_of_day = dto.TimeOfDay,
                frequency = dto.Frequency
            };
        }

        public static void ApplyUpdate(this RoutineStep step, RoutineStepUpdateDto dto)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.StepOrder.HasValue)
            {
                step.step_order = dto.StepOrder.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Instruction))
            {
                step.instruction = dto.Instruction;
            }

            if (!string.IsNullOrWhiteSpace(dto.TimeOfDay))
            {
                step.time_of_day = dto.TimeOfDay;
            }

            if (!string.IsNullOrWhiteSpace(dto.Frequency))
            {
                step.frequency = dto.Frequency;
            }
        }
    }
}
