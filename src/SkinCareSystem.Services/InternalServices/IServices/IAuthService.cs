using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Auth;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    public interface IAuthService
    {
        Task<IServiceResult> LoginAsync(LoginRequestDto request);
    }
}
