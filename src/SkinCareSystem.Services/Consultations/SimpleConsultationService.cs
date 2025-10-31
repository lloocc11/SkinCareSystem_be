using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.Services.Consultations;

public class SimpleConsultationService : ISimpleConsultationService
{
    private const string SystemPrompt = """
Bạn là chuyên gia da liễu người Việt. Hãy tư vấn giàu thông tin nhưng dễ hiểu dựa trên mô tả và hình ảnh (nếu có).
- Đánh giá nhanh mức độ và vấn đề chính.
- Giải thích nguyên nhân khả dĩ.
- Đề xuất tối đa 4 khuyến nghị cụ thể (sản phẩm, thói quen, lưu ý).
- Nhắc người dùng liên hệ bác sĩ nếu tình trạng kéo dài hoặc nghiêm trọng.
Luôn trả lời bằng tiếng Việt, giọng chuyên nghiệp, tinh tế và giàu sự đồng cảm.
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

    public async Task<SimpleConsultationResponse> GenerateAdviceAsync(Guid userId, string text, string? imageUrl, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var userExists = await _dbContext.Users.AnyAsync(u => u.user_id == userId, ct).ConfigureAwait(false);
        if (!userExists)
        {
            throw new InvalidOperationException($"User {userId} was not found.");
        }

        var prompt = BuildUserPrompt(text, imageUrl);

        string advice;
        try
        {
            advice = await _llmClient.ChatPlainAsync(SystemPrompt, prompt, imageUrl, _chatModel, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate simple consultation for user {UserId}", userId);
            throw;
        }

        var cleaned = advice.Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            throw new InvalidOperationException("LLM returned an empty response.");
        }

        return new SimpleConsultationResponse
        {
            Advice = cleaned,
            Model = _chatModel,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }

    private static string BuildUserPrompt(string text, string? imageUrl)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Thông tin người dùng cung cấp:");
        builder.AppendLine(text.Trim());

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            builder.AppendLine();
            builder.AppendLine($"Ảnh tham chiếu: {imageUrl}");
            builder.AppendLine("Nếu ảnh truy cập được, hãy mô tả tình trạng da và lồng ghép vào tư vấn.");
        }

        builder.AppendLine();
        builder.AppendLine("Cấu trúc câu trả lời gợi ý:");
        builder.AppendLine("1. Tổng quan & đánh giá mức độ.");
        builder.AppendLine("2. Giải thích nguyên nhân có thể.");
        builder.AppendLine("3. Gợi ý chăm sóc & sản phẩm cụ thể.");
        builder.AppendLine("4. Lưu ý hoặc cảnh báo.");
        builder.AppendLine("5. Nhắc nhở tham khảo bác sĩ khi cần.");

        return builder.ToString();
    }
}
