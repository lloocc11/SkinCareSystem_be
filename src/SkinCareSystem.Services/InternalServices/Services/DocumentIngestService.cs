using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Pgvector;
using SkinCareSystem.Common.DTOs.MedicalDocument;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Common.Utils;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.Services.InternalServices.Services;

public class DocumentIngestService : IDocumentIngestService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive"
    };

    private static readonly HashSet<string> TextLikeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".md", ".markdown", ".pdf", ".doc", ".docx", ".csv", ".tsv", ".xlsx"
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ITextExtractorService _textExtractorService;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<DocumentIngestService> _logger;

    public DocumentIngestService(
        IUnitOfWork unitOfWork,
        ICloudinaryService cloudinaryService,
        ITextExtractorService textExtractorService,
        IEmbeddingService embeddingService,
        ILogger<DocumentIngestService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        _textExtractorService = textExtractorService ?? throw new ArgumentNullException(nameof(textExtractorService));
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ServiceResult> IngestAsync(IngestDocumentRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.Title) ||
            request.Files == null ||
            request.Files.Count == 0)
        {
            return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Title and at least one file are required.");
        }

        var normalizedStatus = NormalizeStatus(request.Status);
        if (normalizedStatus == null)
        {
            return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Status must be either 'active' or 'inactive'.");
        }

        var validFiles = request.Files.Where(f => f?.Length > 0).ToList();
        if (validFiles.Count == 0)
        {
            return new ServiceResult(Const.ERROR_VALIDATION_CODE, "No uploadable files were provided.");
        }

        var now = DateTimeHelper.UtcNowUnspecified();
        var document = new MedicalDocument
        {
            doc_id = Guid.NewGuid(),
            title = request.Title.Trim(),
            source = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source.Trim(),
            status = normalizedStatus,
            content = string.Empty,
            created_at = now,
            last_updated = now
        };

        var aggregatedText = new StringBuilder();
        await _unitOfWork.MedicalDocuments.CreateAsync(document);

        var assetCount = 0;
        foreach (var file in validFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var uploadResult = await _cloudinaryService
                    .UploadFileAsync(file, "medical-documents", cancellationToken)
                    .ConfigureAwait(false);

                var asset = new MedicalDocumentAsset
                {
                    asset_id = Guid.NewGuid(),
                    doc_id = document.doc_id,
                    file_url = string.IsNullOrWhiteSpace(uploadResult.SecureUrl)
                        ? uploadResult.Url
                        : uploadResult.SecureUrl,
                    public_id = uploadResult.PublicId,
                    provider = "cloudinary",
                    mime_type = file.ContentType,
                    size_bytes = (int)Math.Min(uploadResult.Bytes, int.MaxValue),
                    width = uploadResult.Width > 0 ? uploadResult.Width : null,
                    height = uploadResult.Height > 0 ? uploadResult.Height : null,
                    created_at = now
                };

                await _unitOfWork.MedicalDocumentAssets.CreateAsync(asset);
                assetCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload asset for document {DocumentId}.", document.doc_id);
                return new ServiceResult(Const.ERROR_EXCEPTION, "Failed to upload one of the files.");
            }

            if (IsTextLike(file))
            {
                try
                {
                    var extracted = await _textExtractorService
                        .ExtractTextAsync(file)
                        .ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(extracted))
                    {
                        aggregatedText.AppendLine(extracted.Trim());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Extraction failed for file {FileName}. Continuing without its content.", file.FileName);
                }
            }
        }

        document.content = aggregatedText.ToString().Trim();
        await _unitOfWork.SaveAsync();

        var response = new IngestDocumentResponseDto
        {
            DocumentId = document.doc_id,
            AssetCount = assetCount,
            IngestStatus = string.IsNullOrWhiteSpace(document.content) ? "assets_uploaded" : "queued"
        };

        return new ServiceResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, response);
    }

    public async Task<ServiceResult> EmbedAsync(Guid documentId, EmbedDocumentRequestDto request, CancellationToken cancellationToken = default)
    {
        if (documentId == Guid.Empty)
        {
            return new ServiceResult(Const.ERROR_VALIDATION_CODE, "DocumentId is invalid.");
        }

        var document = await _unitOfWork.MedicalDocuments.GetByIdAsync(documentId);
        if (document == null)
        {
            return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Medical document not found.");
        }

        var normalizedContent = document.content?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedContent))
        {
            return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Document content is empty. Upload text-based files first.");
        }

        var chunkSize = Math.Max(200, request?.ChunkSize ?? 1000);
        var overlap = Math.Clamp(request?.ChunkOverlap ?? 150, 0, chunkSize - 1);

        var chunks = ChunkContent(normalizedContent, chunkSize, overlap).ToList();
        if (chunks.Count == 0)
        {
            return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Unable to chunk document content. Try reducing chunk size.");
        }

        await _unitOfWork.DocumentChunks.DeleteByDocumentIdAsync(documentId);

        var now = DateTimeHelper.UtcNowUnspecified();
        foreach (var chunkText in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            float[] vector;
            try
            {
                vector = await _embeddingService
                    .GetEmbeddingAsync(chunkText, request?.EmbeddingModel)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Embedding failed for document {DocumentId}.", documentId);
                return new ServiceResult(Const.ERROR_EXCEPTION, "Embedding service failed.");
            }

            var chunk = new DocumentChunk
            {
                chunk_id = Guid.NewGuid(),
                doc_id = documentId,
                chunk_text = chunkText,
                embedding = new Vector(vector),
                created_at = now
            };

            await _unitOfWork.DocumentChunks.CreateAsync(chunk);
        }

        document.last_updated = now;
        await _unitOfWork.MedicalDocuments.UpdateAsync(document);
        await _unitOfWork.SaveAsync();

        var response = new EmbedDocumentResponseDto
        {
            ChunkCount = chunks.Count
        };

        return new ServiceResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, response);
    }

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "active";
        }

        var normalized = status.Trim().ToLowerInvariant();
        return AllowedStatuses.Contains(normalized) ? normalized : null;
    }

    private static bool IsTextLike(IFormFile file)
    {
        if (file == null || string.IsNullOrWhiteSpace(file.FileName))
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName);
        return !string.IsNullOrWhiteSpace(extension) && TextLikeExtensions.Contains(extension);
    }

    private static IEnumerable<string> ChunkContent(string content, int chunkSize, int overlap)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            yield break;
        }

        var normalized = content.ReplaceLineEndings("\n");
        var start = 0;
        while (start < normalized.Length)
        {
            var length = Math.Min(chunkSize, normalized.Length - start);
            var end = start + length;

            if (end < normalized.Length)
            {
                var lastSentenceBreak = normalized.LastIndexOfAny(new[] { '.', '!', '?', '\n' }, end - 1, length);
                if (lastSentenceBreak > start + chunkSize / 2)
                {
                    end = lastSentenceBreak + 1;
                }
            }

            var chunk = normalized[start..end].Trim();
            if (!string.IsNullOrWhiteSpace(chunk))
            {
                yield return chunk;
            }

            if (end >= normalized.Length)
            {
                break;
            }

            start = Math.Max(0, end - overlap);
            if (start >= normalized.Length)
            {
                break;
            }
        }
    }
}
