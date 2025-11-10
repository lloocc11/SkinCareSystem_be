using System;
using System.Threading.Tasks;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.Services.InternalServices.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly IEmbeddingClient _embeddingClient;

    public EmbeddingService(IEmbeddingClient embeddingClient)
    {
        _embeddingClient = embeddingClient ?? throw new ArgumentNullException(nameof(embeddingClient));
    }

    public Task<float[]> GetEmbeddingAsync(string text, string? model = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Embedding text cannot be empty.", nameof(text));
        }

        return _embeddingClient.EmbedAsync(text, model);
    }
}
