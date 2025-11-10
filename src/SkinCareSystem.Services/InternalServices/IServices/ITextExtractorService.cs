using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SkinCareSystem.Services.InternalServices.IServices;

public interface ITextExtractorService
{
    Task<string> ExtractTextAsync(IFormFile file);
}
