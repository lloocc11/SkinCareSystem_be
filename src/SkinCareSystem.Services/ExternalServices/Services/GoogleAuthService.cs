using Microsoft.Extensions.Logging;
using SkinCareSystem.Common.DTOs.AuthDTOs;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.Services.ExternalServices.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ILogger<GoogleAuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<IServiceResult> AuthenticateWithGoogleAsync(GoogleAuthRequestDto request)
    {
        try
        {
            // 1. Extract user info from request (already authenticated by frontend)
            var googleId = request.GoogleId;
            var email = request.Email;
            var fullName = request.FullName;

            // 2. Validate input
            if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Google ID and email are required",
                    Data = null
                };
            }

            // 3. Check if user exists by Google ID
            var existingUser = await _unitOfWork.UserRepository.GetByGoogleIdAsync(googleId);

            User user;
            bool isNewUser = false;

            if (existingUser != null)
            {
                // User exists - update last login
                user = existingUser;
                if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = email;
                }

                if (!string.IsNullOrWhiteSpace(fullName) && !string.Equals(user.FullName, fullName, StringComparison.Ordinal))
                {
                    user.FullName = fullName;
                }

                user.UpdatedAt = DateTime.Now;
                await _unitOfWork.UserRepository.UpdateAsync(user);
            }
            else
            {
                // Check if email already exists (registered with different method)
                var emailUser = await _unitOfWork.UserRepository.GetByEmailAsync(email);
                if (emailUser != null)
                {
                    return new ServiceResult
                    {
                        Status = Const.WARNING_DATA_EXISTED_CODE,
                        Message = Const.ERROR_EMAIL_EXISTS_MSG,
                        Data = null
                    };
                }

                // Get default "user" role
                var userRole = await _unitOfWork.RoleRepository.GetRoleByNameAsync("user");
                if (userRole == null)
                {
                    _logger.LogError("Default 'user' role not found in database");
                    return new ServiceResult
                    {
                        Status = Const.ERROR_EXCEPTION,
                        Message = "System configuration error: default role not found",
                        Data = null
                    };
                }

                // Create new user
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    FullName = string.IsNullOrWhiteSpace(fullName) ? email : fullName,
                    Email = email,
                    GoogleId = googleId,
                    RoleId = userRole.RoleId,
                    Status = "active",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _unitOfWork.UserRepository.CreateAsync(user);
                isNewUser = true;
            }

            await _unitOfWork.SaveAsync();

            // 4. Get role information
            var role = await _unitOfWork.RoleRepository.GetByIdAsync(user.RoleId);

            // 5. Generate JWT token
            var jwtResult = _jwtService.GenerateToken(user);

            // 6. Return response
            var response = new GoogleAuthResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = role?.Name ?? "user",
                AccessToken = jwtResult.Token,
                RefreshToken = string.Empty, // Not using refresh tokens for now
                ExpiresAt = jwtResult.ExpiresAtUtc
            };

            return new ServiceResult
            {
                Status = isNewUser ? Const.SUCCESS_REGISTER_CODE : Const.SUCCESS_LOGIN_CODE,
                Message = isNewUser ? Const.SUCCESS_REGISTER_MSG : Const.SUCCESS_LOGIN_MSG,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            return new ServiceResult
            {
                Status = Const.ERROR_EXCEPTION,
                Message = "An error occurred during authentication",
                Data = null
            };
        }
    }
}
