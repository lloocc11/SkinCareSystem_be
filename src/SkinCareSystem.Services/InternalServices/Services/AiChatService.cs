using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;
using SkinCareSystem.Services.Options;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.Services.InternalServices.Services
{
    /// <summary>
    /// Provides realtime AI chat inside an existing chat session using RAG + OpenAI.
    /// </summary>
    public class AiChatService : IAiChatService
    {
        private const int ContextCharacterLimit = 3500;
        private static readonly string[] DefaultSourceFilter = { "guideline:vn-2024", "faq" };

        private const string SystemPrompt = """
Bạn là chuyên gia da liễu nói tiếng Việt. Phải đưa ra lời khuyên ngắn gọn, dựa trên bằng chứng từ phần "Tri thức liên quan". Luôn giữ giọng điệu đồng cảm, rõ ràng cho người dùng phổ thông.

Yêu cầu xuất ra JSON hợp lệ với các trường:
- summary (chuỗi tóm tắt tình trạng)
- recommendations (mảng, mỗi phần tử có type, title, details). BẮT BUỘC phải có phần tử type = "disclaimer".
- routine (tùy chọn) gồm description và steps[]. Mỗi step có order, instruction, time_of_day (morning/evening/both), frequency (daily/twice_daily/weekly/as_needed).
- confidence (0..1)
""";

        private const string JsonSchema = """
{
  "type": "object",
  "properties": {
    "summary": { "type": "string" },
    "recommendations": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "type": { "type": "string" },
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
          }
        }
      }
    },
    "confidence": { "type": "number", "minimum": 0, "maximum": 1 }
  },
  "required": ["summary", "recommendations", "confidence"]
}
""";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IRagSearchService _ragSearchService;
        private readonly ILlmClient _llmClient;
        private readonly ICloudinaryService? _cloudinaryService;
        private readonly ILogger<AiChatService> _logger;
        private readonly OpenAISettings _openAiSettings;

        public AiChatService(
            IUnitOfWork unitOfWork,
            IRagSearchService ragSearchService,
            ILlmClient llmClient,
            ILogger<AiChatService> logger,
            IOptions<OpenAISettings> openAiSettings,
            ICloudinaryService? cloudinaryService = null)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _ragSearchService = ragSearchService ?? throw new ArgumentNullException(nameof(ragSearchService));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _openAiSettings = openAiSettings?.Value ?? new OpenAISettings();
            _cloudinaryService = cloudinaryService;
        }

        /// <inheritdoc />
        public async Task<ServiceResult> ChatInSessionAsync(ChatMessageCreateDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
            {
                return new ServiceResult(Const.ERROR_INVALID_DATA_CODE, Const.ERROR_INVALID_DATA_MSG);
            }

            var normalizedContent = (dto.Content ?? string.Empty).Trim();
            var hasImageFile = dto.Image is { Length: > 0 };
            var hasImageUrl = !string.IsNullOrWhiteSpace(dto.ImageUrl);

            if (string.IsNullOrWhiteSpace(normalizedContent) && !hasImageFile && !hasImageUrl)
            {
                return new ServiceResult(Const.ERROR_INVALID_DATA_CODE, "Message content or image is required.");
            }

            var session = await _unitOfWork.ChatSessionRepository.GetByIdAsync(dto.SessionId).ConfigureAwait(false);
            if (session == null)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Chat session not found.");
            }

            if (session.user_id != dto.UserId)
            {
                return new ServiceResult(Const.FORBIDDEN_ACCESS_CODE, Const.FORBIDDEN_ACCESS_MSG);
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(dto.UserId).ConfigureAwait(false);
            if (user == null)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "User not found.");
            }

            string? uploadedImageUrl = dto.ImageUrl;
            if (hasImageFile)
            {
                uploadedImageUrl = await UploadImageAsync(dto.Image!, cancellationToken).ConfigureAwait(false);
            }

            dto.ImageUrl = uploadedImageUrl;
            dto.Content = normalizedContent;

            var hasText = !string.IsNullOrWhiteSpace(normalizedContent);
            var hasImage = !string.IsNullOrWhiteSpace(uploadedImageUrl);
            var messageType = DetermineMessageType(hasText, hasImage);

            var timestamp = DateTimeHelper.UtcNowUnspecified();

            await using var transaction = await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                var userMessageEntity = dto.ToEntity("user", messageType, timestamp);
                await _unitOfWork.ChatMessageRepository.CreateAsync(userMessageEntity).ConfigureAwait(false);

                var contextItems = await _ragSearchService.SearchAsync(
                        hasText ? normalizedContent : "Phân tích ảnh người dùng cung cấp",
                        topK: 6,
                        sourceFilter: DefaultSourceFilter,
                        ct: cancellationToken)
                    .ConfigureAwait(false);

                var contextText = BuildContext(contextItems);
                var prompt = BuildUserPrompt(normalizedContent, uploadedImageUrl, contextText, contextItems);

                string llmResponseJson;
                try
                {
                    llmResponseJson = await _llmClient.ChatJsonAsync(
                            SystemPrompt,
                            prompt,
                            JsonSchema,
                            _openAiSettings.ChatModel,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OpenAI chat failed for session {SessionId}", dto.SessionId);
                    throw;
                }

                var responseObject = ParseResponse(llmResponseJson);
                EnsureDisclaimer(responseObject);

                var assistantContent = BuildAssistantMessage(responseObject);
                var confidence = ExtractConfidence(responseObject);

                var analysis = new AIAnalysis
                {
                    analysis_id = Guid.NewGuid(),
                    user_id = dto.UserId,
                    chat_message_id = userMessageEntity.message_id,
                    raw_input = normalizedContent,
                    result = responseObject.ToJsonString(new JsonSerializerOptions
                    {
                        WriteIndented = false
                    }),
                    confidence = confidence ?? 0.7,
                    created_at = timestamp,
                    updated_at = timestamp
                };
                await _unitOfWork.AIAnalysisRepository.CreateAsync(analysis).ConfigureAwait(false);

                Guid? routineId = null;
                var routineGenerated = false;
                if (dto.GenerateRoutine && TryExtractRoutine(responseObject, out var routineInfo, out var stepsArray))
                {
                    var routineResult = await PersistRoutineAsync(dto.UserId, analysis.analysis_id, routineInfo, stepsArray, cancellationToken).ConfigureAwait(false);
                    if (routineResult != Guid.Empty)
                    {
                        routineId = routineResult;
                        routineGenerated = true;
                    }
                }

                var assistantMessageEntity = ChatMessageMapper.CreateAssistantMessage(
                    dto.SessionId,
                    assistantContent,
                    null,
                    timestamp);
                await _unitOfWork.ChatMessageRepository.CreateAsync(assistantMessageEntity).ConfigureAwait(false);

                await _unitOfWork.SaveAsync().ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                var responseDto = new ChatTurnResponseDto
                {
                    UserMessage = userMessageEntity.ToDto(),
                    AssistantMessage = assistantMessageEntity.ToDto(),
                    AnalysisId = analysis.analysis_id,
                    RoutineId = routineId,
                    RoutineGenerated = routineGenerated,
                    Confidence = confidence
                };
                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, responseDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogError(ex, "Failed to process AI chat turn for session {SessionId}", dto.SessionId);
                return new ServiceResult(Const.ERROR_EXCEPTION, $"Không thể xử lý yêu cầu chat: {ex.Message}");
            }
        }

        private async Task<Guid> PersistRoutineAsync(Guid userId, Guid analysisId, JsonObject routineInfo, JsonArray stepsArray, CancellationToken ct)
        {
            if (!stepsArray.Any())
            {
                return Guid.Empty;
            }

            var routineId = Guid.NewGuid();
            var steps = CreateRoutineSteps(stepsArray, routineId);
            if (steps.Count == 0)
            {
                return Guid.Empty;
            }

            var timestamp = DateTimeHelper.UtcNowUnspecified();
            var routine = new Routine
            {
                routine_id = routineId,
                user_id = userId,
                analysis_id = analysisId,
                description = routineInfo["description"]?.GetValue<string>() ?? "Routine cá nhân hoá",
                status = "active",
                version = 1,
                created_at = timestamp,
                updated_at = timestamp
            };
            await _unitOfWork.RoutineRepository.CreateAsync(routine).ConfigureAwait(false);

            foreach (var step in steps)
            {
                await _unitOfWork.RoutineStepRepository.CreateAsync(step).ConfigureAwait(false);
            }

            return routineId;
        }

        private static List<RoutineStep> CreateRoutineSteps(JsonArray stepsArray, Guid routineId)
        {
            var steps = new List<RoutineStep>();
            var orderFallback = 1;
            foreach (var node in stepsArray.OfType<JsonObject>())
            {
                var instruction = node["instruction"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(instruction))
                {
                    continue;
                }

                int resolvedOrder = orderFallback++;
                if (node.TryGetPropertyValue("order", out var orderNode) &&
                    orderNode is JsonValue orderValue &&
                    orderValue.TryGetValue<int>(out var parsedOrder) &&
                    parsedOrder > 0)
                {
                    resolvedOrder = parsedOrder;
                }

                var step = new RoutineStep
                {
                    step_id = Guid.NewGuid(),
                    routine_id = routineId,
                    step_order = resolvedOrder,
                    instruction = instruction.Trim(),
                    time_of_day = SanitizeTimeOfDay(node["time_of_day"]?.GetValue<string>()),
                    frequency = SanitizeFrequency(node["frequency"]?.GetValue<string>())
                };

                steps.Add(step);
            }

            return steps
                .OrderBy(s => s.step_order)
                .ThenBy(s => s.step_id)
                .ToList();
        }

        private static string DetermineMessageType(bool hasText, bool hasImage)
        {
            return (hasText, hasImage) switch
            {
                (true, true) => "mixed",
                (false, true) => "image",
                _ => "text"
            };
        }

        private async Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken)
        {
            if (_cloudinaryService != null)
            {
                try
                {
                    var upload = await _cloudinaryService.UploadFileAsync(file, "skincare_system/chat", cancellationToken).ConfigureAwait(false);
                    return string.IsNullOrWhiteSpace(upload.SecureUrl) ? upload.Url : upload.SecureUrl;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cloudinary upload failed, fallback to local storage.");
                }
            }

            var uploadsRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads", "chat-images");
            Directory.CreateDirectory(uploadsRoot);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(uploadsRoot, fileName);

            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
            }

            return $"/uploads/chat-images/{fileName}";
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
                entry.AppendLine($"[{index + 1}] Nguồn: {item.Source ?? "không rõ"}");
                if (!string.IsNullOrWhiteSpace(item.Title))
                {
                    entry.AppendLine($"Tiêu đề: {item.Title}");
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

        private static string BuildUserPrompt(string content, string? imageUrl, string context, IReadOnlyList<RagItem> items)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Thông tin người dùng:");
            if (!string.IsNullOrWhiteSpace(content))
            {
                builder.AppendLine(content);
            }
            else
            {
                builder.AppendLine("Người dùng chỉ cung cấp hình ảnh.");
            }

            builder.AppendLine();

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                builder.AppendLine($"Ảnh người dùng cung cấp: {imageUrl}");
            }

            var assetUrls = items
                .SelectMany(i => i.AssetUrls ?? Array.Empty<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (assetUrls.Length > 0)
            {
                builder.AppendLine("Hình ảnh tham khảo kèm theo tri thức:");
                foreach (var url in assetUrls.Take(5))
                {
                    builder.AppendLine($"- {url}");
                }
            }

            builder.AppendLine();
            builder.AppendLine("Tri thức liên quan:");
            builder.AppendLine(context);
            builder.AppendLine("Hãy phản hồi bằng JSON hợp lệ theo schema yêu cầu.");

            return builder.ToString();
        }

        private static JsonObject ParseResponse(string json)
        {
            var node = JsonNode.Parse(json) as JsonObject;
            if (node == null)
            {
                throw new InvalidOperationException("LLM response is not a valid JSON object.");
            }

            return node;
        }

        private static void EnsureDisclaimer(JsonObject response)
        {
            if (!response.TryGetPropertyValue("recommendations", out var node) || node is not JsonArray recommendations)
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

        private static string BuildAssistantMessage(JsonObject response)
        {
            var builder = new StringBuilder();

            if (response.TryGetPropertyValue("summary", out var summaryNode))
            {
                var summary = summaryNode?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    builder.AppendLine($"Tóm tắt: {summary.Trim()}");
                }
            }

            if (response.TryGetPropertyValue("recommendations", out var recommendationsNode) &&
                recommendationsNode is JsonArray recommendations)
            {
                var nonDisclaimer = recommendations
                    .OfType<JsonObject>()
                    .Where(r => !string.Equals(r["type"]?.GetValue<string>(), "disclaimer", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (nonDisclaimer.Count > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine("Khuyến nghị:");
                    foreach (var recommendation in nonDisclaimer)
                    {
                        var title = recommendation["title"]?.GetValue<string>();
                        var details = recommendation["details"]?.GetValue<string>();
                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            builder.AppendLine($"- {title.Trim()}: {details?.Trim()}");
                        }
                    }
                }

                var disclaimer = recommendations
                    .OfType<JsonObject>()
                    .FirstOrDefault(r => string.Equals(r["type"]?.GetValue<string>(), "disclaimer", StringComparison.OrdinalIgnoreCase));
                if (disclaimer != null)
                {
                    var details = disclaimer["details"]?.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(details))
                    {
                        builder.AppendLine();
                        builder.AppendLine($"Lưu ý: {details.Trim()}");
                    }
                }
            }

            return builder.ToString().Trim();
        }

        private static bool TryExtractRoutine(JsonObject response, out JsonObject routineInfo, out JsonArray steps)
        {
            routineInfo = new JsonObject();
            steps = new JsonArray();

            if (!response.TryGetPropertyValue("routine", out var routineNode) || routineNode is not JsonObject routine)
            {
                return false;
            }

            routineInfo = routine;

            if (routine.TryGetPropertyValue("steps", out var stepsNode) && stepsNode is JsonArray array && array.Count > 0)
            {
                steps = array;
                return true;
            }

            return false;
        }

        private static double? ExtractConfidence(JsonObject response)
        {
            if (!response.TryGetPropertyValue("confidence", out var node) || node == null)
            {
                return null;
            }

            try
            {
                return node.Deserialize<double?>();
            }
            catch (JsonException)
            {
                if (double.TryParse(node.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    return Math.Clamp(value, 0.0, 1.0);
                }
            }

            return null;
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

        private static DateTime ToDatabaseTimestamp(DateTime value) => DateTimeHelper.EnsureUnspecified(value);
    }
}
