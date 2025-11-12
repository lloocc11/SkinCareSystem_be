using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.APIService.Controllers;

/// <summary>
/// RAG search endpoint: trả về chunk tài liệu + ảnh liên quan để phục vụ tư vấn.
/// </summary>
[Authorize]
[ApiController]
[Route("rag")]
public sealed class RagController : BaseApiController
{
    private static readonly ServiceResult RagDisabled = new(410, "RAG search is disabled. Use published routine templates for AI suggestions.");

    /// <summary>
    /// POST /rag/queries - Semantic search trên tài liệu y khoa (pgvector + Cloudinary assets).
    /// </summary>
    [HttpPost("queries")]
    public IActionResult SearchAsync() => ToHttpResponse(RagDisabled);
}
