using System;

namespace SkinCareSystem.Common.DTOs.Media
{
    public class MediaUploadResultDto
    {
        public string Url { get; set; } = string.Empty;
        public string SecureUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public long Bytes { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? ResourceType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
