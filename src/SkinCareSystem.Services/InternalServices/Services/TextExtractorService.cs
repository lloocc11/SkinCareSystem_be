using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SkinCareSystem.Services.InternalServices.IServices;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace SkinCareSystem.Services.InternalServices.Services;

public class TextExtractorService : ITextExtractorService
{
    private readonly ILogger<TextExtractorService> _logger;

    public TextExtractorService(ILogger<TextExtractorService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> ExtractTextAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return string.Empty;
        }

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        switch (extension)
        {
            case ".txt":
            case ".md":
            case ".markdown":
            case ".csv":
            case ".tsv":
                return await ReadPlainTextAsync(file).ConfigureAwait(false);
            case ".pdf":
                return await ReadPdfTextAsync(file).ConfigureAwait(false);
            case ".docx":
                return await ReadDocxTextAsync(file).ConfigureAwait(false);
            case ".doc":
                _logger.LogWarning("Định dạng .doc chưa được hỗ trợ, vui lòng chuyển sang .docx hoặc .pdf.");
                return string.Empty;
            default:
                _logger.LogWarning("Unsupported file extension {Extension}", extension);
                return string.Empty;
        }
    }

    private static async Task<string> ReadPlainTextAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
        var content = await reader.ReadToEndAsync().ConfigureAwait(false);
        return content?.Trim() ?? string.Empty;
    }

    private static async Task<string> ReadPdfTextAsync(IFormFile file)
    {
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream).ConfigureAwait(false);
        memoryStream.Position = 0;

        var builder = new StringBuilder();
        using (var pdfReader = new PdfReader(memoryStream))
        using (var pdfDocument = new PdfDocument(pdfReader))
        {
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                var text = PdfTextExtractor.GetTextFromPage(page, strategy);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    builder.AppendLine(text.Trim());
                }
            }
        }

        return builder.ToString().Trim();
    }

    private static async Task<string> ReadDocxTextAsync(IFormFile file)
    {
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream).ConfigureAwait(false);
        memoryStream.Position = 0;

        using var document = WordprocessingDocument.Open(memoryStream, false);
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var paragraph in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
        {
            var text = paragraph.InnerText;
            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.AppendLine(text.Trim());
            }
        }

        return builder.ToString().Trim();
    }
}
