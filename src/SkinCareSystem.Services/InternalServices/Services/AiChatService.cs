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
using SkinCareSystem.Common.Constants;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.InternalServices.Models;
using SkinCareSystem.Services.Mapping;
using SkinCareSystem.Services.Options;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.Services.InternalServices.Services
{
    /// <summary>
    /// Provides realtime AI chat inside an existing chat session powered by OpenAI (templates-only).
    /// </summary>
    public class AiChatService : IAiChatService
    {
        private const string SystemPrompt = """
Bạn là chuyên gia da liễu nói tiếng Việt. Luôn dựa trên kiến thức y khoa công khai và danh sách routine template đã publish được cung cấp ở cuối bản nhắc (không sử dụng tài liệu nội bộ).

Nhiệm vụ:
1. Tóm tắt tình trạng da trong 2-3 câu dễ hiểu.
2. Tạo mảng recommendations (type/title/details). Bắt buộc có ít nhất 1 recommendation với type="disclaimer".
3. Đánh giá danh sách routine template đã cho, chọn tối đa 3 routine phù hợp nhất (sử dụng đúng giá trị id). Với mỗi routine được chọn, mô tả ngắn gọn (shortDescription) và giải thích vì sao phù hợp (whyMatched) dựa trên loại da, vấn đề chính hoặc mô tả của người dùng.
4. Trả về confidence từ 0..1 phản ánh độ chắc chắn của phân tích.

Chỉ trả về JSON hợp lệ theo schema đã cung cấp.
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
    "routines": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "id": { "type": "string" },
          "title": { "type": "string" },
          "shortDescription": { "type": "string" },
          "whyMatched": { "type": "string" }
        },
        "required": ["id", "whyMatched"]
      },
      "minItems": 0,
      "maxItems": 3
    },
    "confidence": { "type": "number", "minimum": 0, "maximum": 1 }
  },
  "required": ["summary", "recommendations", "routines", "confidence"]
}
""";

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILlmClient _llmClient;
        private readonly ICloudinaryService? _cloudinaryService;
        private readonly IRoutineRecommendationService _routineRecommendationService;
        private readonly ILogger<AiChatService> _logger;
        private readonly OpenAISettings _openAiSettings;

        public AiChatService(
            IUnitOfWork unitOfWork,
            ILlmClient llmClient,
            IRoutineRecommendationService routineRecommendationService,
            ILogger<AiChatService> logger,
            IOptions<OpenAISettings> openAiSettings,
            ICloudinaryService? cloudinaryService = null)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
            _routineRecommendationService = routineRecommendationService ?? throw new ArgumentNullException(nameof(routineRecommendationService));
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

            if (!string.Equals(session.channel, ChatSessionChannels.Ai, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(session.channel, ChatSessionChannels.AiAdmin, StringComparison.OrdinalIgnoreCase))
            {
                return new ServiceResult(Const.ERROR_INVALID_DATA_CODE, "AI chat is not available for this channel.");
            }

            if (string.Equals(session.state, ChatSessionStates.Closed, StringComparison.OrdinalIgnoreCase))
            {
                return new ServiceResult(Const.WARNING_DATA_EXISTED_CODE, "Session is already closed.");
            }

            if (!dto.UserId.HasValue)
            {
                return new ServiceResult(Const.ERROR_INVALID_DATA_CODE, "UserId is required.");
            }

            if (session.user_id != dto.UserId.Value)
            {
                return new ServiceResult(Const.FORBIDDEN_ACCESS_CODE, Const.FORBIDDEN_ACCESS_MSG);
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(dto.UserId.Value).ConfigureAwait(false);
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
            var userTimestamp = DateTimeHelper.UtcNowUnspecified();
            var userMessageEntity = dto.ToEntity(messageType, userTimestamp);
            await _unitOfWork.ChatMessageRepository.CreateAsync(userMessageEntity).ConfigureAwait(false);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            IReadOnlyList<RoutineRecommendation> routineCandidates;
            try
            {
                routineCandidates = await _routineRecommendationService
                    .RecommendAsync(
                        hasText ? normalizedContent : "Phân tích ảnh người dùng cung cấp",
                        user.skin_type,
                        topK: 5,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Routine recommendation failed for session {SessionId}", dto.SessionId);
                return new ServiceResult(Const.ERROR_EXCEPTION, "Không thể xử lý yêu cầu do lỗi gợi ý routine.");
            }

            var prompt = BuildUserPrompt(
                normalizedContent,
                uploadedImageUrl,
                routineCandidates);

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
                return new ServiceResult(Const.ERROR_EXCEPTION, "Không thể xử lý yêu cầu chat: AI không phản hồi.");
            }

            var responseObject = ParseResponse(llmResponseJson);
            EnsureDisclaimer(responseObject);
            var routineSuggestions = MapRoutineSuggestions(responseObject, routineCandidates);
            if (routineSuggestions.Count == 0 && routineCandidates.Count > 0)
            {
                routineSuggestions = CreateFallbackSuggestions(routineCandidates);
                InjectRoutineSuggestions(responseObject, routineSuggestions);
            }

            var assistantContent = BuildAssistantMessage(responseObject, routineSuggestions);
            var confidence = ExtractConfidence(responseObject);

            var analysisTimestamp = DateTimeHelper.UtcNowUnspecified();
            var assistantTimestamp = DateTimeHelper.UtcNowUnspecified();
            if (assistantTimestamp <= userTimestamp)
            {
                assistantTimestamp = userTimestamp.AddTicks(1);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                var analysis = new AIAnalysis
                {
                    analysis_id = Guid.NewGuid(),
                    user_id = dto.UserId.Value,
                    chat_message_id = userMessageEntity.message_id,
                    raw_input = normalizedContent,
                    result = responseObject.ToJsonString(new JsonSerializerOptions
                    {
                        WriteIndented = false
                    }),
                    confidence = confidence ?? 0.7,
                    created_at = analysisTimestamp,
                    updated_at = analysisTimestamp
                };
                await _unitOfWork.AIAnalysisRepository.CreateAsync(analysis).ConfigureAwait(false);

                var assistantMessageEntity = ChatMessageMapper.CreateAssistantMessage(
                    dto.SessionId,
                    assistantContent,
                    null,
                    assistantTimestamp);
                await _unitOfWork.ChatMessageRepository.CreateAsync(assistantMessageEntity).ConfigureAwait(false);

                await _unitOfWork.SaveAsync().ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                var responseDto = new ChatTurnResponseDto
                {
                    UserMessage = userMessageEntity.ToDto(),
                    AssistantMessage = assistantMessageEntity.ToDto(),
                    AnalysisId = analysis.analysis_id,
                    RoutineId = null,
                    RoutineGenerated = false,
                    Confidence = confidence,
                    SuggestedRoutines = routineSuggestions
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
        private static string BuildUserPrompt(
            string content,
            string? imageUrl,
            IReadOnlyList<RoutineRecommendation> routineCandidates)
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

            builder.AppendLine();
            builder.AppendLine("Chỉ sử dụng kiến thức da liễu công khai và danh sách routine template đã publish dưới đây để đưa ra khuyến nghị.");
            builder.AppendLine();
            builder.AppendLine(BuildRoutineCandidateSection(routineCandidates));
            builder.AppendLine("Hãy phản hồi bằng JSON hợp lệ theo schema yêu cầu.");

            return builder.ToString();
        }

        private static string BuildRoutineCandidateSection(IReadOnlyList<RoutineRecommendation> candidates)
        {
            if (candidates.Count == 0)
            {
                return "Hiện chưa có routine template nào phù hợp trong hệ thống.";
            }

            var builder = new StringBuilder();
            builder.AppendLine("Danh sách routine template đã xuất bản (chỉ chọn tối đa 3 và giữ nguyên giá trị id):");
            foreach (var candidate in candidates)
            {
                builder.AppendLine($"- id: {candidate.RoutineId}");
                builder.AppendLine($"  title: {candidate.Title}");
                builder.AppendLine($"  short_description: {candidate.ShortDescription}");
                builder.AppendLine($"  target_skin_type: {candidate.TargetSkinType ?? "phù hợp mọi loại da"}");
                builder.AppendLine($"  target_conditions: {candidate.TargetConditions ?? "đa mục tiêu"}");
                if (candidate.Steps.Count > 0)
                {
                    builder.AppendLine("  key_steps:");
                    foreach (var step in candidate.Steps.Take(4))
                    {
                        builder.AppendLine($"    • {step.time_of_day}: {step.instruction?.Trim()} ({step.frequency})");
                    }
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static IReadOnlyList<RoutineSuggestionDto> MapRoutineSuggestions(
            JsonObject response,
            IReadOnlyList<RoutineRecommendation> candidates)
        {
            if (candidates.Count == 0 ||
                !response.TryGetPropertyValue("routines", out var routinesNode) ||
                routinesNode is not JsonArray routineArray ||
                routineArray.Count == 0)
            {
                return Array.Empty<RoutineSuggestionDto>();
            }

            var lookup = candidates.ToDictionary(r => r.RoutineId, r => r);
            var result = new List<RoutineSuggestionDto>();

            foreach (var item in routineArray.OfType<JsonObject>())
            {
                var idValue = item["id"]?.GetValue<string>();
                if (!Guid.TryParse(idValue, out var routineId))
                {
                    continue;
                }

                if (!lookup.TryGetValue(routineId, out var candidate))
                {
                    continue;
                }

                var title = item["title"]?.GetValue<string>();
                var shortDescription = item["shortDescription"]?.GetValue<string>();
                var whyMatched = item["whyMatched"]?.GetValue<string>();

                result.Add(new RoutineSuggestionDto
                {
                    RoutineId = routineId,
                    Title = string.IsNullOrWhiteSpace(title) ? candidate.Title : title!.Trim(),
                    ShortDescription = string.IsNullOrWhiteSpace(shortDescription)
                        ? candidate.ShortDescription
                        : shortDescription!.Trim(),
                    TargetSkinType = candidate.TargetSkinType,
                    TargetConditions = candidate.TargetConditions,
                    WhyMatched = string.IsNullOrWhiteSpace(whyMatched) ? candidate.InitialReason : whyMatched!.Trim(),
                    SimilarityScore = Math.Round(candidate.SimilarityScore, 4)
                });
            }

            return result;
        }

        private static List<RoutineSuggestionDto> CreateFallbackSuggestions(IReadOnlyList<RoutineRecommendation> candidates)
        {
            return candidates
                .Take(3)
                .Select(candidate => new RoutineSuggestionDto
                {
                    RoutineId = candidate.RoutineId,
                    Title = candidate.Title,
                    ShortDescription = candidate.ShortDescription,
                    TargetSkinType = candidate.TargetSkinType,
                    TargetConditions = candidate.TargetConditions,
                    WhyMatched = candidate.InitialReason,
                    SimilarityScore = Math.Round(candidate.SimilarityScore, 4)
                })
                .ToList();
        }

        private static void InjectRoutineSuggestions(JsonObject response, IReadOnlyList<RoutineSuggestionDto> suggestions)
        {
            var array = new JsonArray();
            foreach (var suggestion in suggestions)
            {
                array.Add(new JsonObject
                {
                    ["id"] = suggestion.RoutineId.ToString(),
                    ["title"] = suggestion.Title,
                    ["shortDescription"] = suggestion.ShortDescription,
                    ["whyMatched"] = suggestion.WhyMatched ?? "Được đề xuất dựa trên ngữ cảnh."
                });
            }

            response["routines"] = array;
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

        private static string BuildAssistantMessage(JsonObject response, IReadOnlyList<RoutineSuggestionDto> suggestions)
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

            if (suggestions.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Routine template gợi ý:");
                foreach (var suggestion in suggestions)
                {
                    var reason = string.IsNullOrWhiteSpace(suggestion.WhyMatched)
                        ? "Phù hợp với nhu cầu đã mô tả."
                        : suggestion.WhyMatched.Trim();
                    builder.AppendLine($"- {suggestion.Title}: {reason}");
                }
            }

            return builder.ToString().Trim();
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

        private static DateTime ToDatabaseTimestamp(DateTime value) => DateTimeHelper.EnsureUnspecified(value);
    }
}
