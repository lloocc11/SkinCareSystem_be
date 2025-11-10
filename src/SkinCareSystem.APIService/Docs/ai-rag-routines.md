# AI/RAG Routine APIs

All endpoints below require JWT authentication with role `admin` or `specialist`. Responses follow the standard `ServiceResult` envelope: `{ success, data, message }`.

## Upload & Ingest

### POST `/api/ingest/documents`
- **Auth**: `admin|specialist`
- **Content-Type**: `multipart/form-data`
- **Form fields**
  - `title` (string, required)
  - `source` (string, optional)
  - `status` (`active|inactive`, default `active`)
  - `files[]` (1..n) – allowed extensions: `.txt,.md,.pdf,.doc,.docx,.csv,.tsv,.xlsx,.png,.jpg,.jpeg`
- **Behavior**:
  - Creates a `MedicalDocuments` row.
  - Uploads every file to Cloudinary → `MedicalDocumentAssets`.
  - Extracts plain text from supported files (txt/csv/etc.) and appends to `MedicalDocuments.content`.
  - Returns `ingest_status=queued` if text was extracted, otherwise `assets_uploaded`.
- **Response**
```json
{
  "success": true,
  "data": {
    "documentId": "b6f8e58f-88d5-49da-8ccd-a7ed02f7db23",
    "assetCount": 2,
    "ingestStatus": "queued"
  },
  "message": "Save data success"
}
```

### POST `/api/ingest/documents/{docId}/embed`
- **Auth**: `admin|specialist`
- **Body**
```json
{
  "chunk_size": 1000,
  "chunk_overlap": 150,
  "embedding_model": "text-embedding-3-small"
}
```
- **Behavior**:
  - Deletes previous `DocumentChunks`.
  - Splits document content (respecting overlap) and calls OpenAI embeddings (`Pgvector.Vector` stored).
  - Returns number of chunks created.
- **Response**
```json
{
  "success": true,
  "data": { "chunkCount": 42 },
  "message": "Update data success"
}
```

## AI Routine Generation

Both endpoints fallback to pure LLM when no RAG data is available; `citations` becomes `[]` and `Source="llm"`.

### POST `/api/ai/routines/generate`
- **Auth**: `admin|specialist`
- **Body**
```json
{
  "query": "Routine trị mụn viêm cho da dầu",
  "target_skin_type": "oily",
  "target_conditions": ["acne","inflammation"],
  "k": 12,
  "max_steps": 10,
  "num_variants": 1,
  "auto_save_as_draft": true
}
```
- **Behavior**:
  - Runs semantic search on all active documents.
  - Calls LLM with/without RAG context.
  - Optionally saves a draft routine (status `draft`, type `template`).
- **Response**
```json
{
  "success": true,
  "data": {
    "routineId": "c4a20d70-1c5f-4c52-9216-54e759d4ef17",
    "isRagBased": true,
    "source": "rag",
    "routine": {
      "description": "...disclaimer...",
      "target_skin_type": "oily",
      "target_conditions": ["acne","inflammation"],
      "isRagBased": true,
      "source": "rag",
      "steps": [
        {"order":1,"instruction":"...","timeOfDay":"morning","frequency":"daily"}
      ]
    },
    "citations": [
      {"docId":"...","chunkId":"...","score":0.86}
    ]
  },
  "message": "Save data success"
}
```

### POST `/api/ai/routines/generate-from-docs`
- Same payload as `/generate` but **requires** `doc_ids`.
- Restricts RAG search to those documents; still falls back to LLM if no chunks embedded.

## Routine Lifecycle

### PUT `/api/ai/routines/{routineId}`
- **Auth**: `admin|specialist`
- **Body**
```json
{
  "description": "Updated draft",
  "target_skin_type": "combination",
  "target_conditions": ["acne","sensitivity"],
  "status": "draft",
  "steps": [
    {"order":1,"instruction":"...", "timeOfDay":"evening","frequency":"daily"}
  ]
}
```
- Replaces metadata and steps (existing steps are deleted before reinsert).

### POST `/api/ai/routines/{routineId}/publish`
- Sets status → `published`.

### POST `/api/ai/routines/{routineId}/archive`
- Sets status → `archived`.

## Pipeline Overview

1. **Upload** (`/api/ingest/documents`): store assets + raw text in `MedicalDocuments`.
2. **Embed** (`/api/ingest/documents/{docId}/embed`): chunk + vectorize to `DocumentChunks`.
3. **Generate** (`/api/ai/routines/generate*`): retrieve top-k chunks, call LLM, auto-save draft if requested.
4. **Edit & Publish**:
   - Adjust draft via `PUT /api/ai/routines/{id}`.
   - Publish or archive via the dedicated endpoints.

## Error Notes

- `404` when the target document or routine does not exist.
- `400` on invalid payloads (missing files, invalid status, empty doc_ids).
- Embedding/LLM failures return `500` with safe fallback messages logged server-side.
