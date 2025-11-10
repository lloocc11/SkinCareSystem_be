using System.Threading.Tasks;

namespace SkinCareSystem.Services.InternalServices.IServices;

public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string text, string? model = null);
}
