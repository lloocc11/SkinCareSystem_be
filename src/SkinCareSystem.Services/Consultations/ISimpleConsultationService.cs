using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkinCareSystem.Services.Consultations;

public interface ISimpleConsultationService
{
    Task<SimpleConsultationResult> GenerateAdviceAsync(
        Guid userId,
        string text,
        string? imageUrl,
        bool generateRoutine,
        CancellationToken ct = default);
}
