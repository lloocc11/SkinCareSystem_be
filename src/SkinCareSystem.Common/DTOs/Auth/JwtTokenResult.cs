using System;

namespace SkinCareSystem.Common.DTOs.Auth
{
    public class JwtTokenResult
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
