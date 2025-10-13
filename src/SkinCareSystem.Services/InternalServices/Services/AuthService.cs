using System;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Auth;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;

        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public async Task<IServiceResult> LoginAsync(LoginRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.GoogleId))
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                };
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var normalizedGoogleId = request.GoogleId.Trim();

            var user = await _unitOfWork.UserRepository.GetByEmailAsync(normalizedEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!string.Equals(user.GoogleId, normalizedGoogleId, StringComparison.Ordinal))
            {
                return Unauthorized();
            }

            var tokenResult = _jwtService.GenerateToken(user);
            var response = new LoginResponseDto
            {
                Token = tokenResult.Token,
                ExpiresAtUtc = tokenResult.ExpiresAtUtc
            };

            return new ServiceResult
            {
                Status = Const.SUCCESS_LOGIN_CODE,
                Message = Const.SUCCESS_LOGIN_MSG,
                Data = response
            };
        }

        private static ServiceResult Unauthorized()
        {
            return new ServiceResult
            {
                Status = Const.UNAUTHORIZED_ACCESS_CODE,
                Message = Const.UNAUTHORIZED_ACCESS_MSG
            };
        }
    }
}
