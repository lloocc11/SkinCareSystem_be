namespace SkinCareSystem.Common.DTOs.Question
{
    public class QuestionCreateDto
    {
        public string Text { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Options { get; set; }
    }
}
