namespace SkinCareSystem.Common.DTOs.UserDTOs
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? GoogleId { get; set; }
        public string AuthProvider { get; set; } = string.Empty;
        public bool HasPassword { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? SkinType { get; set; }
        public Guid RoleId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
