using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkinCareSystem.Services.Rag;

public class OpenAILlmClient : ILlmClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAILlmClient> _logger;

    public OpenAILlmClient(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<OpenAILlmClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> ChatJsonAsync(string systemPrompt, string userPrompt, string jsonSchema, string? model = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(systemPrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(userPrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonSchema);

        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI:ApiKey is not configured.");
        }

        using var schemaDocument = JsonDocument.Parse(jsonSchema);

        var chatModel = model ?? _configuration["OpenAI:ChatModel"] ?? "gpt-4o-mini";
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model = chatModel,
            temperature = 0.2,
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "consultation_response",
                    schema = schemaDocument.RootElement
                }
            },
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        });

        var client = _httpClientFactory.CreateClient("openai");
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            _logger.LogError("OpenAI chat request failed with status {StatusCode}: {Body}", response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }

        await using var contentStream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(contentStream, cancellationToken: ct).ConfigureAwait(false);

        var choice = document.RootElement.GetProperty("choices")[0];
        var content = choice.GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("OpenAI chat response did not contain content.");
        }

        return content;
    }
}
