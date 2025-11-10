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
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers;

[Route("api/ai/routines")]
[Authorize(Roles = "admin,specialist")]
public class AiRoutinesController : BaseApiController
{
    private readonly IRagRetriever _ragRetriever;
    private readonly IAiRoutineGeneratorService _aiRoutineGenerator;
    private readonly IRoutineDraftWriter _routineDraftWriter;
    private readonly IAiRoutineManagementService _routineManagementService;
    private readonly ITextExtractorService _textExtractorService;
    private readonly ILogger<AiRoutinesController> _logger;

    public AiRoutinesController(
        IRagRetriever ragRetriever,
        IAiRoutineGeneratorService aiRoutineGenerator,
        IRoutineDraftWriter routineDraftWriter,
        IAiRoutineManagementService routineManagementService,
        ITextExtractorService textExtractorService,
        ILogger<AiRoutinesController> logger)
    {
        _ragRetriever = ragRetriever ?? throw new ArgumentNullException(nameof(ragRetriever));
        _aiRoutineGenerator = aiRoutineGenerator ?? throw new ArgumentNullException(nameof(aiRoutineGenerator));
        _routineDraftWriter = routineDraftWriter ?? throw new ArgumentNullException(nameof(routineDraftWriter));
        _routineManagementService = routineManagementService ?? throw new ArgumentNullException(nameof(routineManagementService));
        _textExtractorService = textExtractorService ?? throw new ArgumentNullException(nameof(textExtractorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateRoutine([FromBody] GenerateRoutineRequestDto request)
    {
        if (!TryGetRequesterId(out var requesterId, out var unauthorizedResponse))
        {
            return unauthorizedResponse!;
        }

        var result = await GenerateInternalAsync(requesterId, request, null).ConfigureAwait(false);
        return ToHttpResponse(result);
    }

    [HttpPost("generate-from-docs")]
    public async Task<IActionResult> GenerateRoutineFromDocs([FromBody] GenerateRoutineRequestDto request)
    {
        if (!TryGetRequesterId(out var requesterId, out var unauthorizedResponse))
        {
            return unauthorizedResponse!;
        }

        if (request.DocumentIds == null || request.DocumentIds.Count == 0)
        {
            return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE, "doc_ids must not be empty."));
        }

        var docFilter = request.DocumentIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (docFilter.Length == 0)
        {
            return ToHttpResponse(new ServiceResult(Const.ERROR_VALIDATION_CODE, "doc_ids must contain valid values."));
        }

        var result = await GenerateInternalAsync(requesterId, request, docFilter).ConfigureAwait(false);
        return ToHttpResponse(result);
    }

    /// <summary>
    /// Tạo routine dựa trên nội dung file upload (không cần ingest trước).
    /// </summary>
    [HttpPost("generate-from-upload")]
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
            .GenerateAsync(baseRequest, Array.Empty<ChunkHit>())
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

    [HttpPost("generate-from-text")]
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
            AutoSaveAsDraft = request.AutoSaveAsDraft
        };

        var draft = await _aiRoutineGenerator
            .GenerateAsync(baseRequest, Array.Empty<ChunkHit>())
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

    private async Task<ServiceResult> GenerateInternalAsync(Guid requesterId, GenerateRoutineRequestDto request, Guid[]? restrictDocIds)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Query))
        {
            return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Query is required.");
        }

        var hits = await _ragRetriever
            .SearchAsync(request.Query, request.K, restrictDocIds, request.EmbeddingModel)
            .ConfigureAwait(false);

        var draft = await _aiRoutineGenerator
            .GenerateAsync(request, hits)
            .ConfigureAwait(false);

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
            Citations = hits
                .Select(hit => new RoutineCitationDto
                {
                    DocId = hit.DocId,
                    ChunkId = hit.ChunkId,
                    Score = hit.Score
                })
                .ToList(),
            IsRagBased = hits.Count > 0,
            Source = hits.Count > 0 ? "rag" : "llm"
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
}
