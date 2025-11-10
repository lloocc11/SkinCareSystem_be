using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.AuthDTOs;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices;

public interface ILocalAuthService
{
    Task<IServiceResult> RegisterAsync(LocalRegisterRequestDto request);
    Task<IServiceResult> LoginAsync(LocalLoginRequestDto request);
}
