using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.AuthDTOs;

public class LocalLoginRequestDto
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
