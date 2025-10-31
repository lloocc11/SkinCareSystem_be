using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.Services.Consultations;

public class SimpleConsultationService : ISimpleConsultationService
{
    private const string SystemPrompt = """
Bạn là chuyên gia da liễu người Việt. Dựa trên mô tả và ảnh (nếu có):
- Đánh giá nhanh tình trạng và mức độ.
- Giải thích nguyên nhân khả dĩ.
- Đưa ra tối đa 4 khuyến nghị cụ thể (sản phẩm, thói quen, lưu ý).
- Luôn nhắc người dùng nên gặp bác sĩ nếu triệu chứng kéo dài hay nặng.
Trả lời bằng JSON hợp lệ theo schema.
""";

    private const string JsonSchema = """
{
  "type": "object",
  "properties": {
    "advice": { "type": "string" },
    "confidence": { "type": "number", "minimum": 0, "maximum": 1 },
    "routine": {
      "anyOf": [
        { "type": "null" },
        {
          "type": "object",
          "properties": {
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
                "required": ["instruction"]
              },
              "minItems": 1
            }
          },
          "required": ["description", "steps"]
        }
      ]
    }
  },
  "required": ["advice"]
}
""";

    private readonly ILlmClient _llmClient;
    private readonly SkinCareSystemDbContext _dbContext;
    private readonly ILogger<SimpleConsultationService> _logger;
    private readonly string _chatModel;

    public SimpleConsultationService(
        ILlmClient llmClient,
        SkinCareSystemDbContext dbContext,
        IConfiguration configuration,
        ILogger<SimpleConsultationService> logger)
    {
        _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _chatModel = configuration?["OpenAI:ChatModel"] ?? "gpt-4o-mini";
    }

    public async Task<SimpleConsultationResult> GenerateAdviceAsync(
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

        var prompt = BuildUserPrompt(text, imageUrl, generateRoutine);

        JsonObject responseObject;
        string advice;
        double confidence;

        try
        {
            var rawResponse = await _llmClient.ChatJsonAsync(SystemPrompt, prompt, JsonSchema, _chatModel, ct).ConfigureAwait(false);
            responseObject = ParseAndNormaliseResponse(rawResponse);
            advice = responseObject["advice"]?.GetValue<string?>()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(advice))
            {
                throw new InvalidOperationException("LLM returned empty advice.");
            }

            confidence = ExtractConfidence(responseObject);
            responseObject["confidence"] = confidence;
            if (!generateRoutine)
            {
                responseObject["routine"] = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate simple consultation for user {UserId}", userId);
            throw;
        }

        var resultJson = responseObject.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        var analysisId = Guid.NewGuid();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        var nowUtc = DateTime.UtcNow;
        var timestamp = ToDatabaseTimestamp(nowUtc);

        var session = new ChatSession
        {
            session_id = Guid.NewGuid(),
            user_id = userId,
            title = $"Tư vấn nhanh {nowUtc:yyyy-MM-dd HH:mm}",
            created_at = timestamp,
            updated_at = timestamp
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
            created_at = timestamp
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
            created_at = timestamp,
            updated_at = timestamp
        };
        _dbContext.AIAnalyses.Add(analysis);

        var routineGenerated = false;
        var routineId = Guid.Empty;

        if (generateRoutine && responseObject["routine"] is JsonObject routineInfo)
        {
            var stepsArray = routineInfo["steps"] as JsonArray;
            if (stepsArray is { Count: > 0 })
            {
                routineId = Guid.NewGuid();
                var routineDescription = routineInfo["description"]?.GetValue<string?>() ?? "Routine cá nhân hoá";
                var routine = new Routine
                {
                    routine_id = routineId,
                    user_id = userId,
                    analysis_id = analysisId,
                    description = routineDescription,
                    status = "active",
                    version = 1,
                    created_at = timestamp,
                    updated_at = timestamp
                };
                _dbContext.Routines.Add(routine);

                var steps = CreateRoutineSteps(stepsArray, routineId);
                if (steps.Count > 0)
                {
                    _dbContext.RoutineSteps.AddRange(steps);
                    routineGenerated = true;
                }
            }
        }

        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);

        return new SimpleConsultationResult
        {
            AnalysisId = analysisId,
            RoutineId = routineGenerated ? routineId : Guid.Empty,
            RoutineGenerated = routineGenerated,
            Advice = advice,
            Model = _chatModel,
            GeneratedAt = DateTimeOffset.UtcNow,
            Json = resultJson,
            Confidence = confidence
        };
    }

    private static string BuildUserPrompt(string text, string? imageUrl, bool generateRoutine)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Thông tin người dùng cung cấp:");
        builder.AppendLine(text.Trim());
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            builder.AppendLine($"Ảnh tham chiếu: {imageUrl}");
            builder.AppendLine("Nếu ảnh khả dụng hãy mô tả tình trạng quan sát được và kết hợp vào tư vấn.");
            builder.AppendLine();
        }

        builder.AppendLine("Bố cục gợi ý cho phần 'advice':");
        builder.AppendLine("1. Tổng quan & đánh giá mức độ.");
        builder.AppendLine("2. Nguyên nhân khả dĩ.");
        builder.AppendLine("3. Khuyến nghị cụ thể.");
        builder.AppendLine("4. Nhắc nhở theo dõi và gặp bác sĩ khi cần.");

        if (generateRoutine)
        {
            builder.AppendLine();
            builder.AppendLine("Người dùng yêu cầu tạo routine cá nhân hoá.");
            builder.AppendLine("Hãy cung cấp một routine gồm 3-6 bước, rõ thời điểm (morning/evening/both) và tần suất.");
        }

        builder.AppendLine();
        builder.AppendLine("Luôn tuân thủ schema JSON đã cho.");
        return builder.ToString();
    }

    private static JsonObject ParseAndNormaliseResponse(string rawResponse)
    {
        var node = JsonNode.Parse(rawResponse, new JsonNodeOptions { PropertyNameCaseInsensitive = false }) as JsonObject;
        if (node is null)
        {
            throw new InvalidOperationException("LLM response is not a JSON object.");
        }

        if (node["advice"] is JsonValue adviceNode)
        {
            var advice = adviceNode.GetValue<string?>() ?? string.Empty;
            node["advice"] = advice.Trim();
        }

        NormaliseRoutine(node);
        return node;
    }

    private static void NormaliseRoutine(JsonObject response)
    {
        if (response["routine"] is not JsonObject routine)
        {
            return;
        }

        if (routine["description"] is JsonValue descriptionNode)
        {
            var description = descriptionNode.GetValue<string?>();
            routine["description"] = string.IsNullOrWhiteSpace(description)
                ? "Routine cá nhân hoá"
                : description.Trim();
        }
        else
        {
            routine["description"] = "Routine cá nhân hoá";
        }

        if (routine["steps"] is not JsonArray steps)
        {
            return;
        }

        var order = 1;
        foreach (var stepNode in steps.OfType<JsonObject>())
        {
            if (stepNode["instruction"] is JsonValue instructionNode)
            {
                var instruction = instructionNode.GetValue<string?>();
                stepNode["instruction"] = instruction?.Trim() ?? string.Empty;
            }

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
            stepNode["time_of_day"] = SanitizeTimeOfDay(stepNode["time_of_day"]?.GetValue<string?>());
            stepNode["frequency"] = SanitizeFrequency(stepNode["frequency"]?.GetValue<string?>());
        }
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

    private static List<RoutineStep> CreateRoutineSteps(JsonArray stepsArray, Guid routineId)
    {
        var steps = new List<RoutineStep>();
        foreach (var node in stepsArray.OfType<JsonObject>())
        {
            var instruction = node["instruction"]?.GetValue<string?>();
            if (string.IsNullOrWhiteSpace(instruction))
            {
                continue;
            }

            var stepOrder = node.TryGetPropertyValue("order", out var orderNode) && orderNode is not null
                ? orderNode.Deserialize<int?>() ?? steps.Count + 1
                : steps.Count + 1;

            var step = new RoutineStep
            {
                step_id = Guid.NewGuid(),
                routine_id = routineId,
                step_order = Math.Max(1, stepOrder),
                instruction = instruction.Trim(),
                time_of_day = SanitizeTimeOfDay(node["time_of_day"]?.GetValue<string?>()),
                frequency = SanitizeFrequency(node["frequency"]?.GetValue<string?>())
            };

            steps.Add(step);
        }

        return steps;
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

    private static DateTime ToDatabaseTimestamp(DateTime value)
    {
        return value.Kind == DateTimeKind.Unspecified
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
    }
}
