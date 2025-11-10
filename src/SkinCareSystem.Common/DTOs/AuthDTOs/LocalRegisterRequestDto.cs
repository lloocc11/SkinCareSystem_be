using System;
using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.AuthDTOs;

public class LocalRegisterRequestDto
{
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [StringLength(100)]
    public string? SkinType { get; set; }

    public DateOnly? DateOfBirth { get; set; }
}
