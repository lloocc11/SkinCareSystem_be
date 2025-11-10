using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SkinCareSystem.Common.DTOs.Auth;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.Options;

namespace SkinCareSystem.Services.ExternalServices.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly SigningCredentials _signingCredentials;
        private readonly TokenValidationParameters _validationParameters;

        public JwtService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured.");
            }
            if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer) || string.IsNullOrWhiteSpace(_jwtSettings.Audience))
            {
                throw new InvalidOperationException("JWT Issuer and Audience must be configured.");
            }
            if (_jwtSettings.ExpirationMinutes <= 0)
            {
                throw new InvalidOperationException("JWT ExpirationMinutes must be greater than zero.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            _validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = securityKey
            };
        }

        public JwtTokenResult GenerateToken(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
            var roleClaimValue = string.IsNullOrWhiteSpace(user.role?.name) ? "user" : user.role!.name;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.user_id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, user.full_name),
                new(ClaimTypes.NameIdentifier, user.user_id.ToString()),
                new("roleId", user.role_id.ToString()),
                new(ClaimTypes.Role, roleClaimValue)
            };

            var tokenDescriptor = new JwtSecurityToken
            (
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: _signingCredentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            return new JwtTokenResult
            {
                Token = token,
                ExpiresAtUtc = expiration
            };
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(token, _validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
