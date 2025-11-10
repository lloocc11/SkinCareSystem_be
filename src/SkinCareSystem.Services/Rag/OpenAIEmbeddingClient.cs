using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkinCareSystem.Services.Rag;

public class OpenAIEmbeddingClient : IEmbeddingClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAIEmbeddingClient> _logger;

    public OpenAIEmbeddingClient(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<OpenAIEmbeddingClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<float[]> EmbedAsync(string text, string? model = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI:ApiKey is not configured.");
        }

        var embeddingModel = model ?? _configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/embeddings");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            input = text,
            model = embeddingModel
        });

        var client = _httpClientFactory.CreateClient("openai");
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            _logger.LogError("OpenAI embedding request failed with status {StatusCode}: {Body}", response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }

        await using var contentStream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(contentStream, cancellationToken: ct).ConfigureAwait(false);

        var embeddingElement = document.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding");

        var vector = new float[embeddingElement.GetArrayLength()];
        var index = 0;
        foreach (var value in embeddingElement.EnumerateArray())
        {
            vector[index++] = value.GetSingle();
        }

        return vector;
    }
}
