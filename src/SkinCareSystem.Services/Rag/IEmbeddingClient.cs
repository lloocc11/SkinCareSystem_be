using System.Threading;
using System.Threading.Tasks;

namespace SkinCareSystem.Services.Rag;

public interface IEmbeddingClient
{
    Task<float[]> EmbedAsync(string text, string? model = null, CancellationToken ct = default);
}
