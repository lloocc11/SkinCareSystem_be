using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkinCareSystem.Common.DTOs.AI;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers;

[Route("api/ai/routines")]
[Authorize(Roles = "admin,specialist")]
public class AiRoutinesController : BaseApiController
{
    private readonly IAiRoutineGeneratorService _aiRoutineGenerator;
    private readonly IRoutineDraftWriter _routineDraftWriter;
    private readonly IAiRoutineManagementService _routineManagementService;
    private readonly ITextExtractorService _textExtractorService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<AiRoutinesController> _logger;

    public AiRoutinesController(
        IAiRoutineGeneratorService aiRoutineGenerator,
        IRoutineDraftWriter routineDraftWriter,
        IAiRoutineManagementService routineManagementService,
        ITextExtractorService textExtractorService,
        ICloudinaryService cloudinaryService,
        ILogger<AiRoutinesController> logger)
    {
        _aiRoutineGenerator = aiRoutineGenerator ?? throw new ArgumentNullException(nameof(aiRoutineGenerator));
        _routineDraftWriter = routineDraftWriter ?? throw new ArgumentNullException(nameof(routineDraftWriter));
        _routineManagementService = routineManagementService ?? throw new ArgumentNullException(nameof(routineManagementService));
        _textExtractorService = textExtractorService ?? throw new ArgumentNullException(nameof(textExtractorService));
        _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Tạo routine AI từ yêu cầu JSON hoặc form-data (có thể kèm hình ảnh).
    /// </summary>
    [HttpPost("drafts")]
    [Consumes("application/json", "multipart/form-data")]
    [RequestSizeLimit(104_857_600)]
    public async Task<IActionResult> GenerateRoutine(
        [FromForm] GenerateRoutineFormRequestDto? formRequest,
        [FromBody] GenerateRoutineRequestDto? jsonRequest)
    {
        if (!TryGetRequesterId(out var requesterId, out var unauthorizedResponse))
        {
            return unauthorizedResponse!;
        }

        if (Request.HasFormContentType)
        {
            var request = formRequest ?? new GenerateRoutineFormRequestDto();
            var generationRequest = new GenerateRoutineRequestDto
            {
                Query = request.Query?.Trim() ?? string.Empty,
                TargetSkinType = request.TargetSkinType,
                TargetConditions = NormalizeConditions(request.TargetConditions),
                MaxSteps = NormalizeMaxSteps(request.MaxSteps ?? 10),
                NumVariants = NormalizeVariantCount(request.NumVariants ?? 1),
                AutoSaveAsDraft = request.AutoSaveAsDraft ?? true
            };

            if (string.IsNullOrWhiteSpace(generationRequest.Query))
            {
                return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE, "query is required."));
            }

            var imageUrls = new List<string>();
            if (request.Images != null)
            {
                foreach (var image in request.Images.Where(img => img != null && img.Length > 0))
                {
                    if (!IsSupportedImage(image, out var extension))
                    {
                        return ToHttpResponse(new ServiceResult(Const.ERROR_INVALID_DATA_CODE,
                            $"Image type '{extension}' is not supported. Use jpg/png/webp."));
                    }

                    try
                    {
                        var upload = await _cloudinaryService
                            .UploadFileAsync(image, "skincare_system/ai-routines")
                            .ConfigureAwait(false);

                        var imageUrl = string.IsNullOrWhiteSpace(upload.SecureUrl) ? upload.Url : upload.SecureUrl;
                        if (!string.IsNullOrWhiteSpace(imageUrl))
                        {
                            imageUrls.Add(imageUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload routine image {FileName}", image.FileName);
                        return ToHttpResponse(new ServiceResult(Const.ERROR_EXCEPTION, "Không thể upload hình ảnh, vui lòng thử lại."));
                    }
                }
            }

            generationRequest.ImageUrls = imageUrls;

            var formResult = await GenerateRoutineAsync(requesterId, generationRequest).ConfigureAwait(false);
            return ToHttpResponse(formResult);
        }

        if (jsonRequest == null)
        {
            return ToHttpResponse(new ServiceResult(Const.ERROR_INVALID_DATA_CODE, Const.ERROR_INVALID_DATA_MSG));
        }

        jsonRequest.TargetConditions = NormalizeConditions(jsonRequest.TargetConditions);
        jsonRequest.MaxSteps = NormalizeMaxSteps(jsonRequest.MaxSteps);
        jsonRequest.NumVariants = NormalizeVariantCount(jsonRequest.NumVariants);
        jsonRequest.ImageUrls ??= new List<string>();

        var jsonResult = await GenerateRoutineAsync(requesterId, jsonRequest).ConfigureAwait(false);
        return ToHttpResponse(jsonResult);
    }

    /// <summary>
    /// Tạo routine dựa trên nội dung file upload (không cần ingest trước).
    /// </summary>
    [HttpPost("drafts/documents")]
    [Authorize(Roles = "admin,specialist")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104_857_600)]
    public async Task<IActionResult> GenerateRoutineFromUpload(
        [FromForm] GenerateRoutineFromUploadRequestDto request,
        [FromForm] List<IFormFile>? files)
    {
        if (!TryGetRequesterId(out var requesterId, out var unauthorizedResponse))
        {
            return unauthorizedResponse!;
        }

        if (files == null || files.Count == 0)
        {
            return ToHttpResponse(new ServiceResult(Const.ERROR_INVALID_DATA_CODE, "At least one file is required."));
        }

        var combinedBuilder = new StringBuilder();
        var extractionSucceeded = false;

        foreach (var file in files)
        {
            if (file == null || file.Length == 0)
            {
                continue;
            }

            if (!IsSupportedUpload(file, out var extension))
            {
                return ToHttpResponse(new ServiceResult(Const.ERROR_INVALID_DATA_CODE, $"File type '{extension}' is not supported."));
            }

            string extracted;
            try
            {
                extracted = await _textExtractorService.ExtractTextAsync(file).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract text from file {FileName}", file.FileName);
                return ToHttpResponse(new ServiceResult(Const.ERROR_EXCEPTION, $"Không thể trích xuất nội dung từ file {file.FileName}."));
            }

            if (string.IsNullOrWhiteSpace(extracted))
            {
                continue;
            }

            extractionSucceeded = true;
            combinedBuilder.AppendLine(extracted.Trim());
            combinedBuilder.AppendLine();
        }

        if (!extractionSucceeded)
        {
            return ToHttpResponse(new ServiceResult(Const.ERROR_INVALID_DATA_CODE, "Không thể trích xuất nội dung từ tài liệu tải lên."));
        }

        var normalizedConditions = NormalizeConditions(request.TargetConditions);
        var baseRequest = new GenerateRoutineRequestDto
        {
            Query = string.IsNullOrWhiteSpace(request.Query)
                ? "Tạo routine dựa trên tài liệu người dùng tải lên"
                : request.Query.Trim(),
            TargetSkinType = request.TargetSkinType,
            TargetConditions = normalizedConditions,
            AdditionalContext = combinedBuilder.ToString().Trim(),
            AutoSaveAsDraft = request.AutoSaveAsDraft
        };

        var draft = await _aiRoutineGenerator
            .GenerateAsync(baseRequest)
            .ConfigureAwait(false);

        draft.IsRagBased = false;
        draft.Source = "llm_upload";

        Guid? routineId = null;
        if (request.AutoSaveAsDraft)
        {
            routineId = await _routineDraftWriter
                .SaveDraftAsync(draft, requesterId, request.TargetSkinType, normalizedConditions)
                .ConfigureAwait(false);
        }

        var response = new GenerateRoutineResponseDto
        {
            RoutineId = routineId,
            Routine = draft,
            Citations = new List<RoutineCitationDto>(),
            IsRagBased = false,
            Source = "llm_upload"
        };

        return ToHttpResponse(new ServiceResult(Const.SUCCESS_CREATE_CODE, "Generate routine from upload success", response));
    }

    [HttpPost("drafts/text")]
    public async Task<IActionResult> GenerateRoutineFromText([FromBody] GenerateRoutineFromTextRequestDto request)
    {
        if (!TryGetRequesterId(out var requesterId, out var unauthorizedResponse))
        {
            return unauthorizedResponse!;
        }

        if (request == null || (string.IsNullOrWhiteSpace(request.Prompt) && string.IsNullOrWhiteSpace(request.Context)))
        {
            return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE, "Prompt hoặc context là bắt buộc."));
        }

        var normalizedConditions = NormalizeConditions(request.TargetConditions);
        var baseRequest = new GenerateRoutineRequestDto
        {
            Query = string.IsNullOrWhiteSpace(request.Prompt)
                ? "Routine được tạo từ văn bản người dùng cung cấp"
                : request.Prompt.Trim(),
            TargetSkinType = request.TargetSkinType,
            TargetConditions = normalizedConditions,
            AdditionalContext = request.Context?.Trim(),
            AutoSaveAsDraft = request.AutoSaveAsDraft,
            MaxSteps = 10,
            NumVariants = 1
        };

        var draft = await _aiRoutineGenerator
            .GenerateAsync(baseRequest)
            .ConfigureAwait(false);

        draft.IsRagBased = false;
        draft.Source = "llm_text";

        Guid? routineId = null;
        if (request.AutoSaveAsDraft)
        {
            routineId = await _routineDraftWriter
                .SaveDraftAsync(draft, requesterId, request.TargetSkinType, normalizedConditions)
                .ConfigureAwait(false);
        }

        var response = new GenerateRoutineResponseDto
        {
            RoutineId = routineId,
            Routine = draft,
            Citations = new List<RoutineCitationDto>(),
            IsRagBased = false,
            Source = "llm_text"
        };

        return ToHttpResponse(new ServiceResult(Const.SUCCESS_CREATE_CODE, "Generate routine from text success", response));
    }

    [HttpPost("{routineId:guid}/publish")]
    public async Task<IActionResult> PublishRoutine(Guid routineId)
    {
        var result = await _routineManagementService.PublishAsync(routineId).ConfigureAwait(false);
        return ToHttpResponse(result);
    }

    [HttpPost("{routineId:guid}/archive")]
    public async Task<IActionResult> ArchiveRoutine(Guid routineId)
    {
        var result = await _routineManagementService.ArchiveAsync(routineId).ConfigureAwait(false);
        return ToHttpResponse(result);
    }

    [HttpPut("{routineId:guid}")]
    public async Task<IActionResult> UpdateRoutine(Guid routineId, [FromBody] AiRoutineUpdateRequestDto request)
    {
        var result = await _routineManagementService.UpdateAsync(routineId, request).ConfigureAwait(false);
        return ToHttpResponse(result);
    }

    private async Task<ServiceResult> GenerateRoutineAsync(Guid requesterId, GenerateRoutineRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Query))
        {
            return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Query is required.");
        }

        request.Query = request.Query.Trim();
        request.AdditionalContext = string.IsNullOrWhiteSpace(request.AdditionalContext)
            ? null
            : request.AdditionalContext.Trim();
        request.TargetConditions = NormalizeConditions(request.TargetConditions);
        request.MaxSteps = NormalizeMaxSteps(request.MaxSteps);
        request.NumVariants = NormalizeVariantCount(request.NumVariants);
        request.ImageUrls ??= new List<string>();

        var draft = await _aiRoutineGenerator.GenerateAsync(request).ConfigureAwait(false);
        draft.IsRagBased = false;
        draft.Source = "llm";

        Guid? routineId = null;
        if (request.AutoSaveAsDraft)
        {
            routineId = await _routineDraftWriter
                .SaveDraftAsync(draft, requesterId, request.TargetSkinType, request.TargetConditions ?? Array.Empty<string>())
                .ConfigureAwait(false);
        }

        var response = new GenerateRoutineResponseDto
        {
            RoutineId = routineId,
            Routine = draft,
            Citations = new List<RoutineCitationDto>(),
            IsRagBased = false,
            Source = "llm"
        };

        return new ServiceResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, response);
    }

    private bool TryGetRequesterId(out Guid requesterId, out IActionResult? unauthorizedResponse)
    {
        requesterId = Guid.Empty;
        unauthorizedResponse = null;

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out requesterId))
        {
            unauthorizedResponse = ToHttpResponse(new ServiceResult(Const.UNAUTHORIZED_ACCESS_CODE, Const.UNAUTHORIZED_ACCESS_MSG));
            return false;
        }

        return true;
    }

    private static bool IsSupportedUpload(IFormFile file, out string extension)
    {
        extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        return extension switch
        {
            ".txt" or ".md" or ".markdown" or ".pdf" or ".doc" or ".docx" or ".csv" or ".tsv" => true,
            _ => false
        };
    }

    private static List<string> NormalizeConditions(IEnumerable<string>? rawConditions)
    {
        var result = new List<string>();
        if (rawConditions == null)
        {
            return result;
        }

        foreach (var value in rawConditions)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var parts = value
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim());

            foreach (var part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    result.Add(part);
                }
            }
        }

        return result;
    }

    private static int NormalizeMaxSteps(int? maxSteps)
    {
        if (!maxSteps.HasValue || maxSteps.Value <= 0)
        {
            return 10;
        }

        return Math.Min(maxSteps.Value, 20);
    }

    private static int NormalizeVariantCount(int? numVariants)
    {
        if (!numVariants.HasValue || numVariants.Value <= 0)
        {
            return 1;
        }

        return Math.Min(numVariants.Value, 3);
    }

    private static bool IsSupportedImage(IFormFile file, out string extension)
    {
        extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        return extension is ".jpg" or ".jpeg" or ".png" or ".webp";
    }
}
