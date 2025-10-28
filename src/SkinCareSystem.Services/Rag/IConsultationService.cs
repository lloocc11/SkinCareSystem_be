using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkinCareSystem.Services.Rag;

public interface IConsultationService
{
    Task<ConsultationResult> CreateConsultationAsync(Guid userId, string text, string? imageUrl, CancellationToken ct = default);
}
