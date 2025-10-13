using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string GoogleId { get; set; } = string.Empty;
    }
}
