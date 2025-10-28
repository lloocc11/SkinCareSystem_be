using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.APIService.Models;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.APIService.Controllers;

[Authorize]
[ApiController]
[Route("consultations")]
public sealed class ConsultationsController : ControllerBase
{
    private readonly IConsultationService _consultationService;
    private readonly IWebHostEnvironment _environment;

    public ConsultationsController(IConsultationService consultationService, IWebHostEnvironment environment)
    {
        _consultationService = consultationService;
        _environment = environment;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<object>>> CreateAsync(
        [FromForm] ConsultationForm form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            var errorResponse = Api.Fail(errorMessage, StatusCodes.Status400BadRequest);
            return StatusCode(errorResponse.Status, errorResponse);
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            var errorResponse = Api.Fail("Không xác định được người dùng.", StatusCodes.Status401Unauthorized);
            return StatusCode(errorResponse.Status, errorResponse);
        }

        string? imageUrl = form.ImageUrl;
        if (form.Image is not null && form.Image.Length > 0)
        {
            imageUrl = await SaveImageAsync(form.Image, cancellationToken).ConfigureAwait(false);
        }

        try
        {
            var result = await _consultationService
                .CreateConsultationAsync(userId, form.Text, imageUrl, cancellationToken)
                .ConfigureAwait(false);

            using var document = JsonDocument.Parse(result.Json);
            var advice = document.RootElement.Clone();

            var payload = new
            {
                result.AnalysisId,
                result.RoutineId,
                result.Confidence,
                advice,
                context = result.ContextItems.Select(RagItemDto.FromDomain).ToArray()
            };

            var response = Api.Created(payload, "Tư vấn đã được tạo.");
            return StatusCode(response.Status, response);
        }
        catch (Exception ex)
        {
            var errorResponse = Api.Fail($"Không thể xử lý tư vấn: {ex.Message}", StatusCodes.Status500InternalServerError);
            return StatusCode(errorResponse.Status, errorResponse);
        }
    }

    private async Task<string> SaveImageAsync(IFormFile file, CancellationToken ct)
    {
        var uploadsRoot = _environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var uploadsPath = Path.Combine(uploadsRoot, "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct).ConfigureAwait(false);
        }

        return $"/uploads/{fileName}";
    }
}
