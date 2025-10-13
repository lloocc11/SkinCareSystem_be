using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.Auth
{
    public class DevLoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
