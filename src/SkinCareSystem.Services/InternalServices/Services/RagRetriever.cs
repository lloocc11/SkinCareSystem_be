using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Rag;

namespace SkinCareSystem.Services.InternalServices.Services;

public class RagRetriever : IRagRetriever
{
    private const string Query = """
        SELECT
            c."doc_id"   AS DocId,
            c."chunk_id" AS ChunkId,
            c."chunk_text" AS Content,
            (c."embedding" <#> @qVec::vector) AS Distance
        FROM "DocumentChunks" c
        JOIN "MedicalDocuments" d ON d."doc_id" = c."doc_id"
        WHERE d."status" = 'active'
          AND (@docIds IS NULL OR c."doc_id" = ANY(@docIds))
        ORDER BY c."embedding" <#> @qVec::vector
        LIMIT @k;
        """;

    private readonly IEmbeddingService _embeddingService;
    private readonly string _connectionString;
    private readonly ILogger<RagRetriever> _logger;

    public RagRetriever(
        IEmbeddingService embeddingService,
        IConfiguration configuration,
        ILogger<RagRetriever> logger)
    {
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
    }

    public async Task<IReadOnlyList<ChunkHit>> SearchAsync(string query, int k, Guid[]? restrictDocIds = null, string? embeddingModel = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<ChunkHit>();
        }

        k = Math.Max(1, Math.Min(k, 50));

        float[] embedding;
        try
        {
            embedding = await _embeddingService.GetEmbeddingAsync(query, embeddingModel).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedding failed for query.");
            return Array.Empty<ChunkHit>();
        }

        var vectorText = PgVectorHelper.ToPgVectorText(embedding);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var parameters = new DynamicParameters();
        parameters.Add("qVec", vectorText, DbType.String);
        parameters.Add("k", k, DbType.Int32);
        parameters.Add("docIds", restrictDocIds != null && restrictDocIds.Length > 0 ? restrictDocIds : null);

        try
        {
            var rows = await connection.QueryAsync<Row>(Query, parameters).ConfigureAwait(false);
            var hits = new List<ChunkHit>();
            foreach (var row in rows)
            {
                var distance = row.Distance;
                var score = Math.Max(0d, 1d - distance);
                hits.Add(new ChunkHit(row.DocId, row.ChunkId, row.Content, score));
            }

            return hits;
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error while running vector search.");
            return Array.Empty<ChunkHit>();
        }
    }

    private sealed record Row(Guid DocId, Guid ChunkId, string Content, double Distance);
}
