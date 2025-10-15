using System;
using System.ComponentModel.DataAnnotations;

namespace SkinCareSystem.Common.DTOs.UserDTOs
{
    public class UserUpdateDto
    {
        [StringLength(200)]
        public string? FullName { get; set; }

        [EmailAddress]
        [StringLength(320)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? SkinType { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public Guid? RoleId { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }
    }
}
