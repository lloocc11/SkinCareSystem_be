using System;

namespace SkinCareSystem.Common.DTOs.Chat
{
    public class ChatTurnResponseDto
    {
        public ChatMessageDto UserMessage { get; set; } = default!;

        public ChatMessageDto AssistantMessage { get; set; } = default!;

        public Guid AnalysisId { get; set; }

        public Guid? RoutineId { get; set; }

        public bool RoutineGenerated { get; set; }

        public double? Confidence { get; set; }
    }
}
