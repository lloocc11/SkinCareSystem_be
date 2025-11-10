using SkinCareSystem.Common.DTOs.AuthDTOs;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.ExternalServices.IServices;

public interface IGoogleAuthService
{
    Task<IServiceResult> AuthenticateWithGoogleAsync(GoogleAuthRequestDto request);
}
