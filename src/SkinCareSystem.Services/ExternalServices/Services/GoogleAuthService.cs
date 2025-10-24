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

            // 3. Normalize input
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var normalizedGoogleId = googleId.Trim();

            // 4. Check if user exists by Google ID
            var existingUser = await _unitOfWork.UserRepository.GetByGoogleIdAsync(normalizedGoogleId);

            User user;
            bool isNewUser = false;

            if (existingUser != null)
            {
                // Existing user - login scenario
                user = existingUser;
                
                // Check if user is active
                if (user.Status?.ToLower() != "active")
                {
                    return new ServiceResult
                    {
                        Status = Const.UNAUTHORIZED_ACCESS_CODE,
                        Message = "Account is inactive. Please contact support.",
                        Data = null
                    };
                }

                // Update email if changed
                if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = normalizedEmail;
                }

                // Update full name if changed
                if (!string.IsNullOrWhiteSpace(fullName) && !string.Equals(user.FullName, fullName, StringComparison.Ordinal))
                {
                    user.FullName = fullName;
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.UserRepository.UpdateAsync(user);
                
                _logger.LogInformation("Existing user logged in: {UserId}, Email: {Email}", user.UserId, user.Email);
            }
            else
            {
                // New user - registration scenario
                // Check if email already exists (registered with different method)
                var emailUser = await _unitOfWork.UserRepository.GetByEmailAsync(normalizedEmail);
                if (emailUser != null)
                {
                    // Email exists but different Google account
                    _logger.LogWarning("Email {Email} already registered with different account", normalizedEmail);
                    return new ServiceResult
                    {
                        Status = Const.WARNING_DATA_EXISTED_CODE,
                        Message = "This email is already registered with another account. Please use a different email or login method.",
                        Data = null
                    };
                }

                // Check if this email should be admin (from environment variable)
                var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
                var shouldBeAdmin = !string.IsNullOrWhiteSpace(adminEmail) && 
                                   string.Equals(normalizedEmail, adminEmail.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);

                // Get appropriate role
                var roleName = shouldBeAdmin ? "admin" : "user";
                var userRole = await _unitOfWork.RoleRepository.GetRoleByNameAsync(roleName);
                
                if (userRole == null)
                {
                    _logger.LogError("{RoleName} role not found in database", roleName);
                    return new ServiceResult
                    {
                        Status = Const.ERROR_EXCEPTION,
                        Message = $"System configuration error: {roleName} role not found",
                        Data = null
                    };
                }

                // Create new user
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    FullName = string.IsNullOrWhiteSpace(fullName) ? normalizedEmail : fullName.Trim(),
                    Email = normalizedEmail,
                    GoogleId = normalizedGoogleId,
                    RoleId = userRole.RoleId,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (shouldBeAdmin)
                {
                    _logger.LogInformation("Creating admin user via Google: {Email}", user.Email);
                }

                await _unitOfWork.UserRepository.CreateAsync(user);
                isNewUser = true;
                
                _logger.LogInformation("New user registered via Google: {UserId}, Email: {Email}", user.UserId, user.Email);
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
