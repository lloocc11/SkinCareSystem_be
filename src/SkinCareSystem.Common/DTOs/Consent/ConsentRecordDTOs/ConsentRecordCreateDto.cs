using System;

namespace SkinCareSystem.Common.DTOs.Consent
{
    public class ConsentRecordCreateDto
    {
        public Guid UserId { get; set; }
        public string ConsentType { get; set; } = null!;
        public string ConsentText { get; set; } = null!;
        public bool Given { get; set; }
    }
}
