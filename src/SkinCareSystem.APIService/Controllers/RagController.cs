using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.APIService.Models;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.APIService.Controllers;

[Authorize]
[ApiController]
[Route("rag")]
public sealed class RagController : ControllerBase
{
    private readonly IRagSearchService _ragSearchService;

    public RagController(IRagSearchService ragSearchService)
    {
        _ragSearchService = ragSearchService;
    }

    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<object>>> SearchAsync(
        [FromBody] RagSearchRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            var errorResponse = Api.Fail(errorMessage, 400);
            return StatusCode(errorResponse.Status, errorResponse);
        }

        var items = await _ragSearchService
            .SearchAsync(request.Query, request.TopK, request.SourceFilter, cancellationToken)
            .ConfigureAwait(false);

        var payload = new
        {
            items = items.Select(RagItemDto.FromDomain).ToArray()
        };

        var response = Api.Ok(payload);
        return StatusCode(response.Status, response);
    }
}
