using System;
using System.Threading;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.MedicalDocument;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices;

public interface IDocumentIngestService
{
    Task<ServiceResult> IngestAsync(IngestDocumentRequestDto request, CancellationToken cancellationToken = default);

    Task<ServiceResult> EmbedAsync(Guid documentId, EmbedDocumentRequestDto request, CancellationToken cancellationToken = default);
}
