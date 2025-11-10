using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkinCareSystem.Services.Rag;

public interface IRagSearchService
{
    Task<IReadOnlyList<RagItem>> SearchAsync(string query, int topK = 6, string[]? sourceFilter = null, CancellationToken ct = default);
}
