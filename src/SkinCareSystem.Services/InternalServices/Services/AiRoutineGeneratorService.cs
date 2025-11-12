using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkinCareSystem.Common.DTOs.AI;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.Services.InternalServices.Services;

public class AiRoutineGeneratorService : IAiRoutineGeneratorService
{
    private const string SystemPrompt = "Bạn là chuyên gia da liễu. Sử dụng kiến thức y khoa công khai và mô tả (bao gồm ảnh) do người dùng cung cấp để đề xuất routine hợp lý. Luôn thêm khuyến cáo đây không thay thế tư vấn bác sĩ.";

    private const string Schema = """
    {
      "type": "object",
      "properties": {
        "description": { "type": "string" },
        "target_skin_type": { "type": ["string", "null"] },
        "target_conditions": {
          "type": "array",
          "items": { "type": "string" }
        },
        "steps": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "order": { "type": "integer" },
              "instruction": { "type": "string" },
              "time_of_day": { "type": "string" },
              "frequency": { "type": "string" }
            },
            "required": [ "instruction" ]
          },
          "minItems": 1
        }
      },
      "required": [ "description", "steps" ]
    }
    """;

    private readonly ILlmClient _llmClient;
    private readonly ILogger<AiRoutineGeneratorService> _logger;

    public AiRoutineGeneratorService(ILlmClient llmClient, ILogger<AiRoutineGeneratorService> logger)
    {
        _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RoutineDraftDto> GenerateAsync(GenerateRoutineRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userPrompt = BuildUserPrompt(request);

        string raw;
        try
        {
            raw = await _llmClient
                .ChatJsonAsync(SystemPrompt, userPrompt, Schema)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM routine generation failed. Falling back to template.");
            return BuildFallbackRoutine(request);
        }

        try
        {
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;

            var draft = new RoutineDraftDto
            {
                Description = root.TryGetProperty("description", out var descriptionProp)
                    ? descriptionProp.GetString() ?? string.Empty
                    : string.Empty,
                TargetSkinType = root.TryGetProperty("target_skin_type", out var skinProp)
                    ? skinProp.GetString()
                    : request.TargetSkinType,
                TargetConditions = ExtractConditions(root, request),
                Steps = ExtractSteps(root),
                IsRagBased = false,
                Source = "llm"
            };

            if (draft.Steps.Count == 0)
            {
                draft.Steps.Add(CreateDefaultStep());
            }

            return draft;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Unable to parse LLM response. Falling back to template.");
            return BuildFallbackRoutine(request);
        }
    }

    private static string BuildUserPrompt(GenerateRoutineRequestDto request)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Yêu cầu tạo routine chăm sóc da với các ràng buộc sau:");
        builder.AppendLine($"- Mục tiêu: {request.Query}");

        if (!string.IsNullOrWhiteSpace(request.TargetSkinType))
        {
            builder.AppendLine($"- Loại da mục tiêu: {request.TargetSkinType}");
        }

        if (request.TargetConditions is { Count: > 0 })
        {
            builder.AppendLine($"- Các vấn đề chính: {string.Join(", ", request.TargetConditions)}");
        }

        builder.AppendLine($"- Tối đa bước: {request.MaxSteps}");
        builder.AppendLine($"- Số routine cần tạo: {Math.Max(1, request.NumVariants)} (nhưng chỉ trả về routine tốt nhất).");
        builder.AppendLine("- Đảm bảo có cảnh báo/disclaimer rõ ràng ở cuối mô tả.");

        if (request.ImageUrls is { Count: > 0 })
        {
            builder.AppendLine();
            builder.AppendLine("Ảnh người dùng cung cấp (tham khảo đường dẫn, không mô tả chi tiết da liễu nếu không chắc chắn):");
            foreach (var url in request.ImageUrls.Take(5))
            {
                builder.AppendLine($"- {url}");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.AdditionalContext))
        {
            builder.AppendLine();
            builder.AppendLine("Ngữ cảnh bổ sung từ tài liệu người dùng cung cấp:");
            builder.AppendLine(TruncateContext(request.AdditionalContext));
            builder.AppendLine();
        }

        builder.AppendLine("Phản hồi JSON với description, target_skin_type, target_conditions[], steps[].");
        return builder.ToString();
    }

    private static IList<string> ExtractConditions(JsonElement root, GenerateRoutineRequestDto request)
    {
        if (root.TryGetProperty("target_conditions", out var conditionsElement) &&
            conditionsElement.ValueKind == JsonValueKind.Array)
        {
            var list = new List<string>();
            foreach (var condition in conditionsElement.EnumerateArray())
            {
                var value = condition.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    list.Add(value.Trim());
                }
            }

            if (list.Count > 0)
            {
                return list;
            }
        }

        return request.TargetConditions?.Where(c => !string.IsNullOrWhiteSpace(c)).ToList()
               ?? new List<string>();
    }

    private static IList<RoutineStepDraftDto> ExtractSteps(JsonElement root)
    {
        var result = new List<RoutineStepDraftDto>();
        if (!root.TryGetProperty("steps", out var stepsElement) || stepsElement.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var stepElement in stepsElement.EnumerateArray())
        {
            if (stepElement.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var instruction = stepElement.TryGetProperty("instruction", out var instructionProp)
                ? instructionProp.GetString()
                : null;

            if (string.IsNullOrWhiteSpace(instruction))
            {
                continue;
            }

            var order = stepElement.TryGetProperty("order", out var orderProp) && orderProp.TryGetInt32(out var value)
                ? value
                : result.Count + 1;

            var timeOfDay = stepElement.TryGetProperty("time_of_day", out var todProp)
                ? todProp.GetString()
                : "morning";

            var frequency = stepElement.TryGetProperty("frequency", out var freqProp)
                ? freqProp.GetString()
                : "daily";

            result.Add(new RoutineStepDraftDto
            {
                Order = order,
                Instruction = instruction.Trim(),
                TimeOfDay = string.IsNullOrWhiteSpace(timeOfDay) ? "morning" : timeOfDay!,
                Frequency = string.IsNullOrWhiteSpace(frequency) ? "daily" : frequency!
            });
        }

        return result
            .OrderBy(s => s.Order)
            .Select((s, index) =>
            {
                s.Order = index + 1;
                return s;
            })
            .ToList();
    }

    private static RoutineDraftDto BuildFallbackRoutine(GenerateRoutineRequestDto request)
    {
        return new RoutineDraftDto
        {
            Description = $"Routine gợi ý cho yêu cầu: {request.Query}\nLưu ý: đây chỉ là khuyến nghị tổng quát, vui lòng tham khảo ý kiến bác sĩ chuyên khoa.",
            TargetSkinType = request.TargetSkinType,
            TargetConditions = request.TargetConditions ?? new List<string>(),
            Steps = new List<RoutineStepDraftDto> { CreateDefaultStep() },
            IsRagBased = false,
            Source = "llm"
        };
    }

    private static RoutineStepDraftDto CreateDefaultStep()
    {
        return new RoutineStepDraftDto
        {
            Order = 1,
            Instruction = "Làm sạch nhẹ nhàng, dưỡng ẩm cân bằng và sử dụng kem chống nắng SPF 30+.",
            TimeOfDay = "morning",
            Frequency = "daily"
        };
    }

    private static string TruncateContext(string text, int maxLength = 6000)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
        {
            return text?.Trim() ?? string.Empty;
        }

        return text[..maxLength].Trim() + "...";
    }
}
