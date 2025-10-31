using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkinCareSystem.Services.Consultations;

public interface ISimpleConsultationService
{
    Task<SimpleConsultationResponse> GenerateAdviceAsync(Guid userId, string text, string? imageUrl, CancellationToken ct = default);
}
