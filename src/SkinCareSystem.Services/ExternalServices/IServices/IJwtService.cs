using System.Security.Claims;
using SkinCareSystem.Common.DTOs.Auth;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.ExternalServices.IServices
{
    public interface IJwtService
    {
        JwtTokenResult GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
