using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.AI;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IAIAnalysisService
    {
        Task<IServiceResult> CreateAnalysisAsync(AIAnalysisCreateDto dto);
        Task<IServiceResult> GetAnalysisByMessageAsync(Guid messageId);
        Task<IServiceResult> GetAnalysesBySessionAsync(Guid sessionId);
    }
}
