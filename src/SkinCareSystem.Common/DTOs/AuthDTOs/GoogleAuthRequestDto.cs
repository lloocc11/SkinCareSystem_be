using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.AuthDTOs;

public class GoogleAuthRequestDto
{
    [Required(ErrorMessage = "Google ID is required")]
    public string GoogleId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    public string FullName { get; set; } = string.Empty;
}
