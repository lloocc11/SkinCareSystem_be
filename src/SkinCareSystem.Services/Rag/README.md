# RAG Services Overview

## Required Configuration

```
ConnectionStrings__DefaultConnection=Host=…;Database=…;Username=…;Password=…;Ssl Mode=Require;Trust Server Certificate=true
OpenAI__ApiKey=<OPENAI_API_KEY>
OpenAI__EmbeddingModel=text-embedding-3-small
OpenAI__ChatModel=gpt-4o-mini
```

The API service must reference `SkinCareSystem.Services.Rag` and call `builder.Services.AddRagServices();`. The extension registers the OpenAI HTTP client, embedding/chat clients, vector search, and consultation workflow.

## Quick Tests (via API controllers)

```
curl -X POST "$API/rag/search" \
  -H "Authorization: Bearer $JWT" \
  -H "Content-Type: application/json" \
  -d '{"query":"Da dầu, mụn ẩn vùng má","topK":6,"sourceFilter":["guideline:vn-2024","faq"]}'
```

```
curl -X POST "$API/consultations" \
  -H "Authorization: Bearer $JWT" \
  -F "text=Da nhạy cảm, mụn ẩn vùng má" \
  -F "image=@/path/to/photo.jpg"
```

Responses follow the envelope:

```
{
  "status": 200,
  "success": true,
  "message": "...",
  "data": { ... },
  "timestamp": "..."
}
```

The consultation workflow stores records in `AIAnalysis`, `Routines`, and `RoutineSteps` within a single transaction; the retrieved context and LLM payload are persisted as JSON strings for traceability. Ensure that the database already contains the `pgvector` extension and HNSW index on `"DocumentChunks"."embedding"`.
