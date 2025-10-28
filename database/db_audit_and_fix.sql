-- db_audit_and_fix.sql
-- Purpose: Audit and repair RAG-related schema elements for the SkinCareSystem database.
-- The script is idempotent; it can be re-run safely to verify and enforce the expected structure.

-- ============================================================
-- SECTION A: AUDIT CHECKS
-- ============================================================

-- Check required extensions.
SELECT extname
FROM pg_extension
WHERE extname IN ('uuid-ossp', 'vector')
ORDER BY extname;

-- Verify primary RAG tables exist.
SELECT
  to_regclass('public."MedicalDocuments"') IS NOT NULL AS medicaldocuments_exists,
  to_regclass('public."DocumentChunks"') IS NOT NULL AS documentchunks_exists;

-- Inspect key columns and data types for MedicalDocuments.
SELECT
  column_name,
  data_type,
  is_nullable,
  collation_name,
  column_default
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name = 'MedicalDocuments'
  AND column_name IN ('doc_id', 'title', 'status', 'source')
ORDER BY column_name;

-- Inspect key columns and data types for DocumentChunks (including embedding vector metadata).
SELECT
  column_name,
  data_type,
  is_nullable,
  udt_name,
  atttypmod
FROM information_schema.columns c
JOIN pg_attribute a
  ON a.attname = c.column_name
  AND a.attrelid = to_regclass('public."DocumentChunks"')
WHERE c.table_schema = 'public'
  AND c.table_name = 'DocumentChunks'
  AND c.column_name IN ('chunk_id', 'doc_id', 'chunk_text', 'embedding')
ORDER BY column_name;

-- Confirm expected check constraint on MedicalDocuments.status.
SELECT conname, pg_get_constraintdef(oid) AS definition
FROM pg_constraint
WHERE conrelid = to_regclass('public."MedicalDocuments"')
  AND contype = 'c'
  AND conname ILIKE '%status%';

-- Gather current indexes on DocumentChunks (expected HNSW using vector cosine ops).
SELECT
  c.relname AS index_name,
  am.amname AS access_method,
  opc.opcname AS operator_class
FROM pg_index i
JOIN pg_class c ON c.oid = i.indexrelid
JOIN pg_am am ON am.oid = c.relam
JOIN pg_opclass opc ON opc.oid = ANY (i.indclass)
WHERE i.indrelid = to_regclass('public."DocumentChunks"')
ORDER BY index_name;

-- Confirm embedding column enforces NOT NULL and uses vector(1536).
SELECT
  att.attnotnull AS embedding_not_null,
  ((att.atttypmod - 4) / 4) AS embedding_dimensions
FROM pg_attribute att
WHERE att.attrelid = to_regclass('public."DocumentChunks"')
  AND att.attname = 'embedding'
  AND att.attnum > 0;

-- Optional uniqueness checks on junction tables.
SELECT conname, pg_get_constraintdef(oid) AS definition
FROM pg_constraint
WHERE conrelid IN (to_regclass('public."UserAnswers"'), to_regclass('public."UserSymptoms"'))
  AND contype = 'u'
ORDER BY conrelid::text, conname;

-- ============================================================
-- SECTION B: FIXES & ENFORCEMENT
-- ============================================================

BEGIN;

-- Ensure required extensions exist.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS vector;

-- Drop HNSW index before reshaping the embedding column to avoid dependency errors.
DROP INDEX IF EXISTS idx_documentchunks_embedding;

-- Ensure DocumentChunks.embedding exists with the correct dimension (vector(1536)).
-- If the column is missing or has the wrong dimension, it is recreated (existing embeddings will be lost).
DO $$
DECLARE
  dim integer;
BEGIN
  SELECT ((att.atttypmod - 4) / 4)
  INTO dim
  FROM pg_attribute att
  WHERE att.attrelid = to_regclass('public."DocumentChunks"')
    AND att.attname = 'embedding'
    AND att.attnum > 0;

  IF dim IS NULL THEN
    RAISE NOTICE 'Column "DocumentChunks.embedding" not found. Creating as vector(1536).';
    EXECUTE 'ALTER TABLE "DocumentChunks" ADD COLUMN "embedding" vector(1536)';
  ELSIF dim <> 1536 THEN
    RAISE NOTICE 'Column "DocumentChunks.embedding" dimension is %, recreating as vector(1536). Existing embedding data will be removed.', dim;
    EXECUTE 'ALTER TABLE "DocumentChunks" DROP COLUMN "embedding"';
    EXECUTE 'ALTER TABLE "DocumentChunks" ADD COLUMN "embedding" vector(1536)';
  END IF;
END;
$$;

-- Populate placeholder zero vectors for any null embeddings introduced by column recreation.
UPDATE "DocumentChunks"
SET "embedding" = array_fill(0::float, ARRAY[1536])::vector(1536)
WHERE "embedding" IS NULL;

-- Enforce NOT NULL on DocumentChunks.embedding (after ensuring the column is correctly typed).
ALTER TABLE "DocumentChunks"
  ALTER COLUMN "embedding" SET NOT NULL;

-- Rebuild HNSW index using cosine distance for embeddings.
CREATE INDEX IF NOT EXISTS idx_documentchunks_embedding
  ON "DocumentChunks"
  USING HNSW ("embedding" vector_cosine_ops);

-- Enforce uniqueness on UserAnswers (user_id, question_id).
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conrelid = to_regclass('public."UserAnswers"')
      AND conname = 'uq_useranswers'
  ) THEN
    ALTER TABLE "UserAnswers"
      ADD CONSTRAINT uq_useranswers UNIQUE ("user_id", "question_id");
  END IF;
END;
$$;

-- Enforce uniqueness on UserSymptoms (user_id, symptom_id).
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conrelid = to_regclass('public."UserSymptoms"')
      AND conname = 'uq_usersymptoms'
  ) THEN
    ALTER TABLE "UserSymptoms"
      ADD CONSTRAINT uq_usersymptoms UNIQUE ("user_id", "symptom_id");
  END IF;
END;
$$;

COMMIT;
