using System;

namespace SkinCareSystem.Common.DTOs.Symptom
{
    public class UserSymptomCreateDto
    {
        public Guid UserId { get; set; }
        public Guid SymptomId { get; set; }
    }
}
