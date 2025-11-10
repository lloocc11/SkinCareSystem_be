using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SkinCareSystem.Common.DTOs.Auth;
using SkinCareSystem.Common.DTOs.AuthDTOs;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.Services.InternalServices.Services;

public class LocalAuthService : ILocalAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<LocalAuthService> _logger;

    public LocalAuthService(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordHasher<User> passwordHasher,
        ILogger<LocalAuthService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IServiceResult> RegisterAsync(LocalRegisterRequestDto request)
    {
        if (request == null)
        {
            return new ServiceResult(Const.ERROR_INVALID_DATA_CODE, Const.ERROR_INVALID_DATA_MSG);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser != null)
        {
            return new ServiceResult(Const.WARNING_DATA_EXISTED_CODE, Const.ERROR_EMAIL_EXISTS_MSG);
        }

        var defaultRole = await _unitOfWork.RoleRepository.GetRoleByNameAsync("user");
        if (defaultRole == null)
        {
            _logger.LogWarning("Default role 'user' not found when registering local account.");
            return new ServiceResult(Const.ERROR_INVALID_DATA_CODE, "Default role is not configured.");
        }

        var user = new User
        {
            user_id = Guid.NewGuid(),
            full_name = request.FullName.Trim(),
            email = normalizedEmail,
            password_hash = string.Empty,
            auth_provider = "local",
            google_id = null,
            role_id = defaultRole.role_id,
            role = defaultRole,
            skin_type = request.SkinType,
            date_of_birth = request.DateOfBirth,
            status = "active",
            created_at = DateTimeHelper.UtcNowUnspecified(),
            updated_at = DateTimeHelper.UtcNowUnspecified()
        };

        user.password_hash = _passwordHasher.HashPassword(user, request.Password.Trim());

        await _unitOfWork.UserRepository.CreateAsync(user);
        await _unitOfWork.SaveAsync();

        var token = _jwtService.GenerateToken(user);
        var response = new LoginResponseDto
        {
            Token = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc
        };

        return new ServiceResult(Const.SUCCESS_REGISTER_CODE, Const.SUCCESS_REGISTER_MSG, response);
    }

    public async Task<IServiceResult> LoginAsync(LocalLoginRequestDto request)
    {
        if (request == null)
        {
            return new ServiceResult(Const.ERROR_INVALID_DATA_CODE, Const.ERROR_INVALID_DATA_MSG);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _unitOfWork.UserRepository.GetByEmailAndProviderAsync(normalizedEmail, "local");
        if (user == null)
        {
            return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Invalid email or password.");
        }

        if (string.IsNullOrWhiteSpace(user.password_hash))
        {
            _logger.LogWarning("Local user {UserId} has empty password hash.", user.user_id);
            return new ServiceResult(Const.ERROR_EXCEPTION, "Local account is misconfigured.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.password_hash, request.Password.Trim());
        if (verification == PasswordVerificationResult.Failed)
        {
            return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Invalid email or password.");
        }

        if (!string.Equals(user.status, "active", StringComparison.OrdinalIgnoreCase))
        {
            return new ServiceResult(Const.UNAUTHORIZED_ACCESS_CODE, "Account is not active.");
        }

        var token = _jwtService.GenerateToken(user);
        var response = new LoginResponseDto
        {
            Token = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc
        };

        return new ServiceResult(Const.SUCCESS_LOGIN_CODE, Const.SUCCESS_LOGIN_MSG, response);
    }
}
