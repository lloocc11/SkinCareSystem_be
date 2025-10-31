using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.Rag;

public class ConsultationService : IConsultationService
{
    private const int ContextCharacterLimit = 3500;

    private static readonly string[] DefaultSourceFilter = { "guideline:vn-2024", "faq" };

    private const string SystemPrompt = """
You are a licensed skincare consultant. Produce evidence-based, empathetic advice in Vietnamese using the provided knowledge snippets. Always include a clear disclaimer reminding the user to consult a healthcare professional when symptoms persist or worsen. Output valid JSON only.
""";

    private const string JsonSchema = """
{
  "type": "object",
  "properties": {
    "analysis_id": { "type": "string" },
    "summary": { "type": "string" },
    "confidence": { "type": "number", "minimum": 0, "maximum": 1 },
    "recommendations": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "type": {
            "type": "string",
            "enum": ["routine", "product", "lifestyle", "warning", "disclaimer"]
          },
          "title": { "type": "string" },
          "details": { "type": "string" }
        },
        "required": ["type", "title", "details"]
      },
      "minItems": 1
    },
    "routine": {
      "type": "object",
      "properties": {
        "name": { "type": "string" },
        "description": { "type": "string" },
        "steps": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "order": { "type": "integer", "minimum": 1 },
              "instruction": { "type": "string" },
              "time_of_day": { "type": "string", "enum": ["morning", "evening", "both"] },
              "frequency": { "type": "string", "enum": ["daily", "twice_daily", "weekly", "as_needed"] }
            },
            "required": ["order", "instruction", "time_of_day", "frequency"]
          },
          "minItems": 1
        }
      },
      "required": ["description", "steps"]
    }
  },
  "required": ["summary", "confidence", "recommendations", "routine"]
}
""";

    private readonly IRagSearchService _ragSearchService;
    private readonly ILlmClient _llmClient;
    private readonly SkinCareSystemDbContext _dbContext;
    private readonly ILogger<ConsultationService> _logger;

    public ConsultationService(
        IRagSearchService ragSearchService,
        ILlmClient llmClient,
        SkinCareSystemDbContext dbContext,
        ILogger<ConsultationService> logger)
    {
        _ragSearchService = ragSearchService;
        _llmClient = llmClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ConsultationResult> CreateConsultationAsync(
        Guid userId,
        string text,
        string? imageUrl,
        bool generateRoutine,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var userExists = await _dbContext.Users.AnyAsync(u => u.user_id == userId, ct).ConfigureAwait(false);
        if (!userExists)
        {
            throw new InvalidOperationException($"User {userId} was not found.");
        }

        var ragItems = await _ragSearchService.SearchAsync(text, 6, DefaultSourceFilter, ct).ConfigureAwait(false);
        var contextText = BuildContext(ragItems);

        var userPrompt = BuildUserPrompt(text, imageUrl, contextText);
        var rawResponse = await _llmClient.ChatJsonAsync(SystemPrompt, userPrompt, JsonSchema, ct: ct).ConfigureAwait(false);

        var responseObject = ParseAndNormaliseResponse(rawResponse);

        var analysisId = Guid.NewGuid();
        responseObject["analysis_id"] = analysisId.ToString();

        var confidence = ExtractConfidence(responseObject);
        var routineInfo = responseObject["routine"] as JsonObject;
        var routineSteps = routineInfo?["steps"] as JsonArray;
        var hasValidSteps = routineSteps is { Count: > 0 };
        var shouldPersistRoutine = generateRoutine && routineInfo is not null && hasValidSteps;

        var routineDescription = routineInfo?["description"]?.GetValue<string>() ?? "Routine cá nhân hoá";
        var routineId = shouldPersistRoutine ? Guid.NewGuid() : Guid.Empty;

        var resultJson = responseObject.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = false
        });

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        var nowUtc = DateTime.UtcNow;
        var nowTimestamp = ToDatabaseTimestamp(nowUtc);

        var session = new ChatSession
        {
            session_id = Guid.NewGuid(),
            user_id = userId,
            title = $"Tư vấn {nowUtc:yyyy-MM-dd HH:mm}",
            created_at = nowTimestamp,
            updated_at = nowTimestamp
        };
        _dbContext.ChatSessions.Add(session);

        var message = new ChatMessage
        {
            message_id = Guid.NewGuid(),
            session_id = session.session_id,
            user_id = userId,
            content = text,
            image_url = imageUrl,
            message_type = DetermineMessageType(text, imageUrl),
            role = "user",
            created_at = nowTimestamp
        };
        _dbContext.ChatMessages.Add(message);

        var analysis = new AIAnalysis
        {
            analysis_id = analysisId,
            user_id = userId,
            chat_message_id = message.message_id,
            raw_input = text,
            result = resultJson,
            confidence = confidence,
            created_at = nowTimestamp,
            updated_at = nowTimestamp
        };
        _dbContext.AIAnalyses.Add(analysis);

        var routineGenerated = false;

        if (shouldPersistRoutine)
        {
            var routine = new Routine
            {
                routine_id = routineId,
                user_id = userId,
                analysis_id = analysisId,
                description = routineDescription,
                status = "active",
                version = 1,
                created_at = nowTimestamp,
                updated_at = nowTimestamp
            };
            _dbContext.Routines.Add(routine);

            var steps = CreateRoutineSteps(routineSteps!, routineId);
            if (steps.Count > 0)
            {
                _dbContext.RoutineSteps.AddRange(steps);
                routineGenerated = true;
            }
        }

        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);

        return new ConsultationResult
        {
            AnalysisId = analysisId,
            RoutineId = routineGenerated ? routineId : Guid.Empty,
            RoutineGenerated = routineGenerated,
            Json = resultJson,
            Confidence = confidence,
            ContextItems = ragItems
        };
    }

    private static string BuildContext(IReadOnlyList<RagItem> items)
    {
        if (items.Count == 0)
        {
            return "Không tìm thấy tri thức phù hợp.";
        }

        var builder = new StringBuilder();
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var entry = new StringBuilder();
            entry.Append($"[{index + 1}] Nguồn: {item.Source ?? "không rõ"}\n");
            if (!string.IsNullOrWhiteSpace(item.Title))
            {
                entry.Append($"Tiêu đề: {item.Title}\n");
            }
            entry.AppendLine("Nội dung:");
            entry.AppendLine(item.Content.Trim());

            if (item.AssetUrls is { Count: > 0 })
            {
                entry.AppendLine("Hình ảnh tham khảo:");
                foreach (var url in item.AssetUrls.Take(3))
                {
                    entry.AppendLine($"- {url}");
                }
            }

            entry.AppendLine("---");

            var entryText = entry.ToString();
            if (builder.Length + entryText.Length > ContextCharacterLimit)
            {
                var remaining = ContextCharacterLimit - builder.Length;
                if (remaining <= 0)
                {
                    break;
                }

                builder.Append(entryText.AsSpan(0, Math.Min(entryText.Length, remaining)));
                break;
            }

            builder.Append(entryText);
        }

        return builder.ToString();
    }

    private static string BuildUserPrompt(string text, string? imageUrl, string context)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Thông tin người dùng:");
        builder.AppendLine(text.Trim());
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            builder.AppendLine($"Ảnh tham chiếu: {imageUrl}");
            builder.AppendLine();
        }

        builder.AppendLine("Tri thức đã truy xuất:");
        builder.AppendLine(context);
        builder.AppendLine("Hãy tổng hợp và trả về JSON hợp lệ theo schema.");
        return builder.ToString();
    }

    private static JsonObject ParseAndNormaliseResponse(string rawResponse)
    {
        var node = JsonNode.Parse(rawResponse, new JsonNodeOptions { PropertyNameCaseInsensitive = false }) as JsonObject;
        if (node is null)
        {
            throw new InvalidOperationException("LLM response is not a JSON object.");
        }

        EnsureDisclaimer(node);
        NormaliseSteps(node);
        return node;
    }

    private static void EnsureDisclaimer(JsonObject response)
    {
        if (!response.TryGetPropertyValue("recommendations", out var recommendationsNode) || recommendationsNode is not JsonArray recommendations)
        {
            recommendations = new JsonArray();
            response["recommendations"] = recommendations;
        }

        var hasDisclaimer = recommendations
            .OfType<JsonObject>()
            .Any(r => string.Equals(r["type"]?.GetValue<string>(), "disclaimer", StringComparison.OrdinalIgnoreCase));

        if (!hasDisclaimer)
        {
            recommendations.Add(new JsonObject
            {
                ["type"] = "disclaimer",
                ["title"] = "Khuyến cáo y khoa",
                ["details"] = "Vui lòng tham khảo bác sĩ da liễu nếu tình trạng kéo dài hoặc nghiêm trọng."
            });
        }
    }

    private static void NormaliseSteps(JsonObject response)
    {
        if (response["routine"] is not JsonObject routine)
        {
            return;
        }

        if (routine["steps"] is not JsonArray steps)
        {
            return;
        }

        var order = 1;
        foreach (var stepNode in steps.OfType<JsonObject>())
        {
            int resolvedOrder;
            if (stepNode.TryGetPropertyValue("order", out var orderNode) && orderNode is not null)
            {
                try
                {
                    resolvedOrder = orderNode.Deserialize<int?>() ?? order++;
                }
                catch (JsonException)
                {
                    resolvedOrder = order++;
                }
            }
            else
            {
                resolvedOrder = order++;
            }

            stepNode["order"] = Math.Max(1, resolvedOrder);

            stepNode["time_of_day"] = SanitizeTimeOfDay(stepNode["time_of_day"]?.GetValue<string>());
            stepNode["frequency"] = SanitizeFrequency(stepNode["frequency"]?.GetValue<string>());
        }
    }

    private static List<RoutineStep> CreateRoutineSteps(JsonArray stepsArray, Guid routineId)
    {
        var steps = new List<RoutineStep>();
        foreach (var node in stepsArray.OfType<JsonObject>())
        {
            var instruction = node["instruction"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(instruction))
            {
                continue;
            }

            var step = new RoutineStep
            {
                step_id = Guid.NewGuid(),
                routine_id = routineId,
                step_order = Math.Max(1, node["order"]?.GetValue<int>() ?? steps.Count + 1),
                instruction = instruction.Trim(),
                time_of_day = SanitizeTimeOfDay(node["time_of_day"]?.GetValue<string>()),
                frequency = SanitizeFrequency(node["frequency"]?.GetValue<string>())
            };

            steps.Add(step);
        }

        return steps;
    }

    private static DateTime ToDatabaseTimestamp(DateTime value)
    {
        // PostgreSQL timestamp without time zone expects DateTime values with an unspecified kind.
        return value.Kind == DateTimeKind.Unspecified
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
    }

    private static string DetermineMessageType(string text, string? imageUrl)
    {
        var hasText = !string.IsNullOrWhiteSpace(text);
        var hasImage = !string.IsNullOrWhiteSpace(imageUrl);

        return (hasText, hasImage) switch
        {
            (true, true) => "mixed",
            (false, true) => "image",
            _ => "text"
        };
    }

    private static string SanitizeTimeOfDay(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "morning" => "morning",
            "evening" => "evening",
            "both" => "both",
            _ => "both"
        };
    }

    private static string SanitizeFrequency(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "daily" => "daily",
            "twice_daily" => "twice_daily",
            "weekly" => "weekly",
            "as_needed" => "as_needed",
            _ => "daily"
        };
    }

    private static double ExtractConfidence(JsonObject response)
    {
        if (!response.TryGetPropertyValue("confidence", out var confidenceNode) || confidenceNode is null)
        {
            return 0.7;
        }

        double? parsed = null;
        try
        {
            parsed = confidenceNode.Deserialize<double?>();
        }
        catch (JsonException)
        {
            if (double.TryParse(confidenceNode.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var manual))
            {
                parsed = manual;
            }
        }

        return Math.Clamp(parsed ?? 0.7, 0, 1);
    }
}
