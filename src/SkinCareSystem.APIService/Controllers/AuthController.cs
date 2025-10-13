using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SkinCareSystem.Common.DTOs.Auth;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IHostEnvironment _environment;

        public AuthController(IAuthService authService, IUnitOfWork unitOfWork, IJwtService jwtService, IHostEnvironment environment)
        {
            _authService = authService;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _environment = environment;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(request);
            return result.Status switch
            {
                Const.SUCCESS_LOGIN_CODE => Ok(new { result.Data, result.Message }),
                Const.UNAUTHORIZED_ACCESS_CODE => Unauthorized(new { result.Message }),
                Const.ERROR_VALIDATION_CODE => BadRequest(new { result.Message }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { result.Message })
            };
        }

        [HttpPost("dev-login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DevLogin([FromBody] DevLoginRequestDto request)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var token = _jwtService.GenerateToken(user);
            return Ok(new LoginResponseDto
            {
                Token = token.Token,
                ExpiresAtUtc = token.ExpiresAtUtc
            });
        }
    }
}
