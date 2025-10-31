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
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.APIService.Controllers;

/// <summary>
/// Tiếp nhận yêu cầu tư vấn (text + ảnh), chạy RAG & LLM để sinh routine cá nhân hóa.
/// <summary>
/// Tiếp nhận yêu cầu tư vấn (text + ảnh), chạy RAG & LLM để sinh routine cá nhân hóa.
/// </summary>
[Authorize]
[ApiController]
[Route("consultations")]
public sealed class ConsultationsController : BaseApiController
{
    private static readonly string[] RoutineKeywords =
    {
        "tạo routine",
        "routine",
        "lộ trình",
        "phác đồ",
        "cho tôi lộ trình",
        "tạo lộ trình"
    };

    private readonly IConsultationService _consultationService;
    private readonly IWebHostEnvironment _environment;
    private readonly ICloudinaryService? _cloudinaryService;

    /// <summary>
    /// Luồng tư vấn cá nhân hóa (text + ảnh) -> RAG -> GPT -> Routine.
    /// </summary>
    public ConsultationsController(
        IConsultationService consultationService,
        IWebHostEnvironment environment,
        ICloudinaryService? cloudinaryService = null)
    {
        _consultationService = consultationService;
        _environment = environment;
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    /// <summary>
    /// Thực hiện tư vấn (RAG + GPT). Đặt <c>GenerateRoutine</c> hoặc dùng từ khóa "tạo routine", "lộ trình", ... để kèm routine.
    /// </summary>
    public async Task<IActionResult> CreateAsync(
        [FromForm] ConsultationForm form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            var invalid = new ServiceResult(Const.ERROR_INVALID_DATA_CODE, errorMessage);
            return ToHttpResponse(invalid);
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            var unauthorized = new ServiceResult(Const.UNAUTHORIZED_ACCESS_CODE, "Không xác định được người dùng.");
            return ToHttpResponse(unauthorized);
        }

        string? imageUrl = form.ImageUrl;
        if (form.Image is not null && form.Image.Length > 0)
        {
            if (_cloudinaryService != null)
            {
                try
                {
                    var upload = await _cloudinaryService.UploadFileAsync(form.Image, "consultations", cancellationToken).ConfigureAwait(false);
                    imageUrl = string.IsNullOrWhiteSpace(upload.SecureUrl) ? upload.Url : upload.SecureUrl;
                }
                catch
                {
                    imageUrl = await SaveImageAsync(form.Image, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                imageUrl = await SaveImageAsync(form.Image, cancellationToken).ConfigureAwait(false);
            }
        }

        try
        {
            var generateRoutine = ShouldGenerateRoutine(form);
            var result = await _consultationService
                .CreateConsultationAsync(userId, form.Text, imageUrl, generateRoutine, cancellationToken)
                .ConfigureAwait(false);

            using var document = JsonDocument.Parse(result.Json);
            var advice = document.RootElement.Clone();

            var payload = new
            {
                result.AnalysisId,
                result.RoutineId,
                result.RoutineGenerated,
                result.Confidence,
                advice,
                context = result.ContextItems.Select(RagItemDto.FromDomain).ToArray()
            };

            var success = new ServiceResult(Const.SUCCESS_CREATE_CODE, "Tư vấn đã được tạo.", payload);
            return ToHttpResponse(success, Url.ActionLink());
        }
        catch (Exception ex)
        {
            var failure = new ServiceResult(Const.ERROR_EXCEPTION, $"Không thể xử lý tư vấn: {ex.Message}");
            return ToHttpResponse(failure);
        }
    }

    private static bool ShouldGenerateRoutine(ConsultationForm form)
    {
        if (form.GenerateRoutine)
        {
            return true;
        }

        var normalized = form.Text?.ToLowerInvariant() ?? string.Empty;
        return RoutineKeywords.Any(keyword => normalized.Contains(keyword));
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
