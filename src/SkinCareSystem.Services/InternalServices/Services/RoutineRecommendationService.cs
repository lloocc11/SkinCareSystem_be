using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.InternalServices.Models;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.Services.InternalServices.Services;

public sealed class RoutineRecommendationService : IRoutineRecommendationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmbeddingClient _embeddingClient;
    public RoutineRecommendationService(
        IUnitOfWork unitOfWork,
        IEmbeddingClient embeddingClient)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _embeddingClient = embeddingClient ?? throw new ArgumentNullException(nameof(embeddingClient));
    }

    public async Task<IReadOnlyList<RoutineRecommendation>> RecommendAsync(
        string userDescription,
        string? userSkinType,
        int topK = 3,
        CancellationToken cancellationToken = default)
    {
        var normalized = (userDescription ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Array.Empty<RoutineRecommendation>();
        }

        var templates = await _unitOfWork.RoutineRepository
            .GetPublishedTemplatesAsync(userSkinType)
            .ConfigureAwait(false);

        if (templates.Count == 0)
        {
            return Array.Empty<RoutineRecommendation>();
        }

        var userVector = await _embeddingClient
            .EmbedAsync(normalized, ct: cancellationToken)
            .ConfigureAwait(false);

        var recommendations = new List<RoutineRecommendation>();
        foreach (var routine in templates)
        {
            var routineText = BuildRoutineEmbeddingText(routine);
            var templateVector = await _embeddingClient
                .EmbedAsync(routineText, ct: cancellationToken)
                .ConfigureAwait(false);

            var similarity = CosineSimilarity(userVector, templateVector);
            var title = BuildRoutineTitle(routine);
            var summary = BuildRoutineSummary(routine);
            var reason = BuildInitialReason(routine, userSkinType, similarity);

            recommendations.Add(new RoutineRecommendation(
                routine.routine_id,
                title,
                summary,
                routine.target_skin_type,
                routine.target_conditions,
                similarity,
                reason,
                routineText,
                routine,
                routine.RoutineSteps?.OrderBy(s => s.step_order).ToArray() ?? Array.Empty<RoutineStep>()));
        }

        return recommendations
            .OrderByDescending(r => r.SimilarityScore)
            .ThenByDescending(r => r.Routine.updated_at ?? r.Routine.created_at)
            .Take(Math.Max(1, topK))
            .ToList();
    }

    private static string BuildRoutineEmbeddingText(Routine routine)
    {
        var builder = new StringBuilder();
        builder.AppendLine(routine.description ?? "Routine chăm sóc da tổng quát.");

        if (!string.IsNullOrWhiteSpace(routine.target_skin_type))
        {
            builder.AppendLine($"Đối tượng da: {routine.target_skin_type}");
        }

        if (!string.IsNullOrWhiteSpace(routine.target_conditions))
        {
            builder.AppendLine($"Vấn đề chính: {routine.target_conditions}");
        }

        if (routine.RoutineSteps != null && routine.RoutineSteps.Count > 0)
        {
            builder.AppendLine("Các bước tiêu biểu:");
            foreach (var step in routine.RoutineSteps.OrderBy(s => s.step_order).Take(4))
            {
                builder.AppendLine($"- {step.time_of_day}: {step.instruction?.Trim()} ({step.frequency})");
            }
        }

        return builder.ToString();
    }

    private static string BuildRoutineTitle(Routine routine)
    {
        if (!string.IsNullOrWhiteSpace(routine.description))
        {
            var firstSentence = routine.description.Split('.', '!', '?')
                .FirstOrDefault(part => !string.IsNullOrWhiteSpace(part));
            if (!string.IsNullOrWhiteSpace(firstSentence))
            {
                return firstSentence.Trim();
            }
        }

        return "Routine chăm sóc da đề xuất";
    }

    private static string BuildRoutineSummary(Routine routine)
    {
        if (string.IsNullOrWhiteSpace(routine.description))
        {
            return "Routine mẫu được thiết kế bởi chuyên gia.";
        }

        var description = routine.description.Trim();
        return description.Length <= 200 ? description : $"{description[..200]}…";
    }

    private static string BuildInitialReason(Routine routine, string? userSkinType, double similarity)
    {
        var reasons = new List<string>();

        if (!string.IsNullOrWhiteSpace(userSkinType) &&
            !string.IsNullOrWhiteSpace(routine.target_skin_type) &&
            string.Equals(routine.target_skin_type, userSkinType, StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("khớp với loại da khai báo");
        }

        if (!string.IsNullOrWhiteSpace(routine.target_conditions))
        {
            reasons.Add($"tập trung vào vấn đề: {routine.target_conditions}");
        }

        if (reasons.Count == 0)
        {
            if (similarity >= 0.8)
            {
                reasons.Add("mức độ tương đồng cao với nhu cầu của bạn");
            }
            else if (similarity >= 0.6)
            {
                reasons.Add("các bước phù hợp với mô tả triệu chứng");
            }
        }

        if (reasons.Count == 0)
        {
            reasons.Add("được đánh giá là phù hợp theo ngữ cảnh tổng thể");
        }

        return string.Join(", ", reasons);
    }

    private static double CosineSimilarity(IReadOnlyList<float> a, IReadOnlyList<float> b)
    {
        if (a.Count != b.Count || a.Count == 0)
        {
            return 0;
        }

        double dot = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;
        for (var i = 0; i < a.Count; i++)
        {
            var x = a[i];
            var y = b[i];
            dot += x * y;
            magnitudeA += x * x;
            magnitudeB += y * y;
        }

        if (magnitudeA <= 0 || magnitudeB <= 0)
        {
            return 0;
        }

        return dot / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }
}
