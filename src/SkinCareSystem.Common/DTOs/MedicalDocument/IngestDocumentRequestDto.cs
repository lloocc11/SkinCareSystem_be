using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SkinCareSystem.Common.DTOs.MedicalDocument;

public class IngestDocumentRequestDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Source { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "active";

    [Required]
    public IList<IFormFile> Files { get; set; } = new List<IFormFile>();
}
