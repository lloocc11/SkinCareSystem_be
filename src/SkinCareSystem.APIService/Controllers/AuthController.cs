using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SkinCareSystem.Common.DTOs.Auth;
using SkinCareSystem.Common.DTOs.AuthDTOs;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// RESTful API endpoints for authentication
    /// </summary>
    [Route("api/auth")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IHostEnvironment _environment;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService, 
            IUnitOfWork unitOfWork, 
            IJwtService jwtService, 
            IHostEnvironment environment,
            IGoogleAuthService googleAuthService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _environment = environment;
            _googleAuthService = googleAuthService;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/auth/token - Standard email/password login
        /// </summary>
        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                });
            }

            var result = await _authService.LoginAsync(request);
            return ToHttpResponse(result);
        }

        /// <summary>
        /// POST /api/auth/tokens/dev - Development-only login (no password required)
        /// </summary>
        [HttpPost("tokens/dev")]
        [AllowAnonymous]
        public async Task<IActionResult> DevLogin([FromBody] DevLoginRequestDto request)
        {
            if (!_environment.IsDevelopment())
            {
                return ToHttpResponse(new ServiceResult(Const.WARNING_NO_DATA_CODE, "Resource not found"));
            }

            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE, Const.ERROR_INVALID_DATA_MSG));
            }

            var user = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
            if (user == null)
            {
                return ToHttpResponse(new ServiceResult(Const.WARNING_NO_DATA_CODE, "User not found"));
            }

            var token = _jwtService.GenerateToken(user);
            return ToHttpResponse(new ServiceResult(Const.SUCCESS_LOGIN_CODE, "Login successful", new LoginResponseDto
            {
                Token = token.Token,
                ExpiresAtUtc = token.ExpiresAtUtc
            }));
        }

        /// <summary>
        /// POST /api/auth/providers/google/token - Login or register with Google
        /// </summary>
        [HttpPost("providers/google/token")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthRequestDto request)
        {
            _logger.LogInformation("Google login attempt for GoogleId: {GoogleId}, Email: {Email}",
                request.GoogleId, request.Email);

            if (!ModelState.IsValid)
            {
                return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE, Const.ERROR_INVALID_DATA_MSG));
            }

            var result = await _googleAuthService.AuthenticateWithGoogleAsync(request);
            
            // For 201 Created, include Location header
            if (result.Status == 201 && result.Data is GoogleAuthResponseDto createdUser)
                return ToHttpResponse(result, $"/api/users/{createdUser.UserId}");

            return ToHttpResponse(result);
        }
    }
}
