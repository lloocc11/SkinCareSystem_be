using System;
using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.UserDTOs
{
    public class UserCreateDto
    {
        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(320)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        public string GoogleId { get; set; } = string.Empty;

        [Required]
        public Guid RoleId { get; set; }

        [StringLength(100)]
        public string? SkinType { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "active";

        public DateOnly? DateOfBirth { get; set; }
    }
}
