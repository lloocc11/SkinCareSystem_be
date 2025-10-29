using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.APIService.Models;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.APIService.Controllers;

/// <summary>
/// RAG search endpoint: trả về chunk tài liệu + ảnh liên quan để phục vụ tư vấn.
/// </summary>
[Authorize]
[ApiController]
[Route("rag")]
public sealed class RagController : BaseApiController
{
    private readonly IRagSearchService _ragSearchService;

    public RagController(IRagSearchService ragSearchService)
    {
        _ragSearchService = ragSearchService;
    }

    /// <summary>
    /// Semantic search trên tài liệu y khoa (pgvector + Cloudinary assets).
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> SearchAsync(
        [FromBody] RagSearchRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            var invalidResult = new ServiceResult(Const.ERROR_INVALID_DATA_CODE, errorMessage);
            return ToHttpResponse(invalidResult);
        }

        var items = await _ragSearchService
            .SearchAsync(request.Query, request.TopK, request.SourceFilter, cancellationToken)
            .ConfigureAwait(false);

        var payload = new
        {
            items = items.Select(RagItemDto.FromDomain).ToArray()
        };

        var result = new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, payload);
        return ToHttpResponse(result);
    }
}
