using System.Threading;
using System.Threading.Tasks;

namespace SkinCareSystem.Services.Rag;

public interface ILlmClient
{
    Task<string> ChatJsonAsync(string systemPrompt, string userPrompt, string jsonSchema, string? model = null, CancellationToken ct = default);
}
