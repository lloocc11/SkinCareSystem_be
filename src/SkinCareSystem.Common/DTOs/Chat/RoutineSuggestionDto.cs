using System;

namespace SkinCareSystem.Common.DTOs.Chat
{
    public class RoutineSuggestionDto
    {
        public Guid RoutineId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ShortDescription { get; set; } = string.Empty;

        public string? TargetSkinType { get; set; }

        public string? TargetConditions { get; set; }

        public string? WhyMatched { get; set; }

        public double? SimilarityScore { get; set; }
    }
}
