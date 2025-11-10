using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
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

    public async Task<string> ChatPlainAsync(string systemPrompt, string text, string? imageUrl = null, string? model = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(systemPrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI:ApiKey is not configured.");
        }

        var chatModel = model ?? _configuration["OpenAI:ChatModel"] ?? "gpt-4o-mini";

        var userContent = new List<object>
        {
            new { type = "text", text }
        };

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            userContent.Add(new
            {
                type = "image_url",
                image_url = new
                {
                    url = imageUrl
                }
            });
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model = chatModel,
            temperature = 0.4,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = new object[]
                    {
                        new { type = "text", text = systemPrompt }
                    }
                },
                new
                {
                    role = "user",
                    content = userContent.ToArray()
                }
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

        var message = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message");

        if (message.TryGetProperty("content", out var contentElement))
        {
            switch (contentElement.ValueKind)
            {
                case JsonValueKind.String:
                    var value = contentElement.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                    break;
                case JsonValueKind.Array:
                    var builder = new StringBuilder();
                    foreach (var segment in contentElement.EnumerateArray())
                    {
                        if (segment.ValueKind != JsonValueKind.Object)
                        {
                            continue;
                        }

                        if (!segment.TryGetProperty("type", out var typeNode))
                        {
                            continue;
                        }

                        if (!string.Equals(typeNode.GetString(), "text", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (segment.TryGetProperty("text", out var textNode))
                        {
                            var segmentText = textNode.GetString();
                            if (!string.IsNullOrWhiteSpace(segmentText))
                            {
                                builder.AppendLine(segmentText);
                            }
                        }
                    }

                    var aggregated = builder.ToString().Trim();
                    if (!string.IsNullOrEmpty(aggregated))
                    {
                        return aggregated;
                    }
                    break;
            }
        }

        throw new InvalidOperationException("OpenAI chat response did not contain content.");
    }
}
