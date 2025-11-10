using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace SkinCareSystem.Services.Rag;

public class RagSearchService : IRagSearchService
{
    private readonly IEmbeddingClient _embeddingClient;
    private readonly string _connectionString;
    private readonly ILogger<RagSearchService> _logger;

    private const string Query = """
        SELECT
            c."chunk_id"     AS ChunkId,
            c."doc_id"       AS DocId,
            d."title"        AS Title,
            d."source"       AS Source,
            c."chunk_text"   AS Content,
            (c."embedding" <#> @qVec::vector) AS Distance
        FROM "DocumentChunks" c
        JOIN "MedicalDocuments" d ON d."doc_id" = c."doc_id"
        WHERE (@src IS NULL OR d."source" = ANY(@src))
          AND d."status" = 'active'
        ORDER BY c."embedding" <#> @qVec::vector
        LIMIT @k;
        """;

    public RagSearchService(
        IEmbeddingClient embeddingClient,
        IConfiguration configuration,
        ILogger<RagSearchService> logger)
    {
        _embeddingClient = embeddingClient;
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
    }

    public async Task<IReadOnlyList<RagItem>> SearchAsync(string query, int topK = 6, string[]? sourceFilter = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        topK = Math.Max(1, topK);

        var embeddings = await _embeddingClient.EmbedAsync(query, ct: ct).ConfigureAwait(false);
        if (embeddings.Length != 1536)
        {
            _logger.LogWarning("Embedding dimension {Dimension} differs from expected 1536.", embeddings.Length);
        }

        var vectorText = PgVectorHelper.ToPgVectorText(embeddings);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        var parameters = new DynamicParameters();
        parameters.Add("qVec", vectorText, DbType.String);
        parameters.Add("k", topK, DbType.Int32);
        if (sourceFilter is { Length: > 0 })
        {
            parameters.Add("src", sourceFilter);
        }
        else
        {
            parameters.Add("src", null);
        }

        var command = new CommandDefinition(Query, parameters, cancellationToken: ct);
        var rows = await connection.QueryAsync<RagRow>(command).ConfigureAwait(false);

        var docIds = rows.Select(r => r.DocId).Distinct().ToArray();
        var assetLookup = new Dictionary<Guid, IReadOnlyList<string>>();

        if (docIds.Length > 0)
        {
            const string assetsSql = """
                SELECT "doc_id" AS DocId, "file_url" AS FileUrl
                FROM "MedicalDocumentAssets"
                WHERE "doc_id" = ANY(@docIds)
                ORDER BY "created_at" DESC;
                """;

            var assetRows = await connection.QueryAsync<AssetRow>(
                new CommandDefinition(assetsSql, new { docIds }, cancellationToken: ct)).ConfigureAwait(false);

            assetLookup = assetRows
                .GroupBy(a => a.DocId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<string>)g.Select(a => a.FileUrl).ToList());
        }

        var items = new List<RagItem>();
        foreach (var row in rows)
        {
            var distance = row.Distance;
            var similarity = Math.Max(0d, 1d - distance);
            assetLookup.TryGetValue(row.DocId, out var assetUrls);
            items.Add(new RagItem(
                row.ChunkId,
                row.DocId,
                row.Title,
                row.Source,
                row.Content,
                distance,
                similarity,
                assetUrls ?? Array.Empty<string>()));
        }

        return items;
    }

    private sealed record RagRow(
        Guid ChunkId,
        Guid DocId,
        string? Title,
        string? Source,
        string Content,
        double Distance);

    private sealed record AssetRow(
        Guid DocId,
        string FileUrl);
}
