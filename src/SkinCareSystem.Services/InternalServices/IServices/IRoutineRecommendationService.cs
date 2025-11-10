using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SkinCareSystem.Services.InternalServices.Models;

namespace SkinCareSystem.Services.InternalServices.IServices;

public interface IRoutineRecommendationService
{
    Task<IReadOnlyList<RoutineRecommendation>> RecommendAsync(
        string userDescription,
        string? userSkinType,
        int topK = 3,
        CancellationToken cancellationToken = default);
}
