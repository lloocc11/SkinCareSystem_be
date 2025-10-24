using System;

namespace SkinCareSystem.Common.DTOs.Symptom
{
    public class UserSymptomDto
    {
        public Guid UserSymptomId { get; set; }
        public Guid UserId { get; set; }
        public Guid SymptomId { get; set; }
        public DateTime? ReportedAt { get; set; }
        public string? UserFullName { get; set; }
        public string? SymptomName { get; set; }
    }
}
