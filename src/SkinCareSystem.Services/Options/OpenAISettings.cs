namespace SkinCareSystem.Services.Options
{
    /// <summary>
    /// Strongly-typed OpenAI configuration bound from OpenAI section.
    /// </summary>
    public class OpenAISettings
    {
        public string? ApiKey { get; set; }

        public string EmbeddingModel { get; set; } = "text-embedding-3-small";

        public string ChatModel { get; set; } = "gpt-4o-mini";
    }
}
