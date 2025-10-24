using System;

namespace SkinCareSystem.Common.DTOs.Consent
{
    public class ConsentRecordDto
    {
        public Guid ConsentId { get; set; }
        public Guid UserId { get; set; }
        public string ConsentType { get; set; } = null!;
        public string ConsentText { get; set; } = null!;
        public bool Given { get; set; }
        public DateTime? GivenAt { get; set; }
        public string? UserFullName { get; set; }
    }
}
