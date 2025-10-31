#!/usr/bin/env python3
"""
Ingest medical knowledge base content into the SkinCareSystem PostgreSQL database.

Usage:
    python scripts/ingest_medical_docs.py \
        --docs-path ./knowledge_base \
        --source "guideline:vn-2024" \
        --status active

Environment variables required:
  - DATABASE_URL (or provide --dsn)
  - GEMINI_API_KEY
Optional for assets:
  - CLOUDINARY_CLOUD_NAME
  - CLOUDINARY_API_KEY
  - CLOUDINARY_API_SECRET
"""

import argparse
import os
import sys
import uuid
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, List, Optional

import psycopg2
import psycopg2.extras

try:
    import google.generativeai as genai
except ImportError:
    genai = None

try:
    import cloudinary
    import cloudinary.uploader
except ImportError:
    cloudinary = None  # Only needed when --upload-assets is used


DEFAULT_CHUNK_WORDS = 220


@dataclass
class DocumentChunk:
    chunk_id: uuid.UUID
    doc_id: uuid.UUID
    text: str
    embedding: List[float]


def chunk_text(content: str, words_per_chunk: int = DEFAULT_CHUNK_WORDS) -> List[str]:
    words = content.split()
    if not words:
        return []
    chunks: List[str] = []
    for i in range(0, len(words), words_per_chunk):
        chunk = " ".join(words[i : i + words_per_chunk]).strip()
        if chunk:
            chunks.append(chunk)
    return chunks


def embed_chunks(chunks: Iterable[str], model_name: str) -> List[List[float]]:
    """Generate embeddings using Gemini Embeddings API."""
    embeddings: List[List[float]] = []
    for chunk in chunks:
        result = genai.embed_content(
            model=model_name,
            content=chunk,
            task_type="retrieval_document",
        )
        embeddings.append(result["embedding"])
    return embeddings


def upload_asset_to_cloudinary(path: Path, folder: str) -> Optional[str]:
    if cloudinary is None:
        raise RuntimeError("cloudinary package not installed. Install `pip install cloudinary` or disable --upload-assets.")
    upload_result = cloudinary.uploader.upload(
        str(path),
        folder=folder,
        resource_type="image",
        use_filename=True,
        unique_filename=True,
    )
    secure_url = upload_result.get("secure_url")
    if not secure_url:
        raise RuntimeError(f"Unable to upload {path} to Cloudinary.")
    return secure_url


def ensure_cloudinary_config():
    required = ["CLOUDINARY_CLOUD_NAME", "CLOUDINARY_API_KEY", "CLOUDINARY_API_SECRET"]
    missing = [env for env in required if not os.getenv(env)]
    if missing:
        raise RuntimeError(f"Missing Cloudinary config: {', '.join(missing)}")
    cloudinary.config(
        cloud_name=os.environ["CLOUDINARY_CLOUD_NAME"],
        api_key=os.environ["CLOUDINARY_API_KEY"],
        api_secret=os.environ["CLOUDINARY_API_SECRET"],
        secure=True,
    )


def insert_document(
    cursor,
    *,
    doc_id: uuid.UUID,
    title: str,
    content: str,
    source: Optional[str],
    status: str,
) -> None:
    cursor.execute(
        """
        INSERT INTO "MedicalDocuments" (doc_id, title, content, source, status)
        VALUES (%s, %s, %s, %s, %s)
        ON CONFLICT (doc_id) DO UPDATE
        SET title = EXCLUDED.title,
            content = EXCLUDED.content,
            source = EXCLUDED.source,
            status = EXCLUDED.status;
        """,
        (doc_id, title, content, source, status),
    )


def insert_chunks(cursor, chunks: List[DocumentChunk]) -> None:
    psycopg2.extras.execute_batch(
        cursor,
        """
        INSERT INTO "DocumentChunks" (chunk_id, doc_id, chunk_text, embedding)
        VALUES (%s, %s, %s, %s)
        ON CONFLICT (chunk_id) DO UPDATE
        SET chunk_text = EXCLUDED.chunk_text,
            embedding = EXCLUDED.embedding;
        """,
        [
            (
                chunk.chunk_id,
                chunk.doc_id,
                chunk.text,
                chunk.embedding,
            )
            for chunk in chunks
        ],
    )


def insert_asset(
    cursor,
    *,
    doc_id: uuid.UUID,
    file_url: str,
    provider: str,
    public_id: Optional[str],
    mime_type: Optional[str],
) -> None:
    asset_id = uuid.uuid4()
    cursor.execute(
        """
        INSERT INTO "MedicalDocumentAssets"
            (asset_id, doc_id, file_url, provider, public_id, mime_type)
        VALUES (%s, %s, %s, %s, %s, %s)
        ON CONFLICT (doc_id, file_url) DO UPDATE
        SET provider = EXCLUDED.provider,
            public_id = EXCLUDED.public_id,
            mime_type = EXCLUDED.mime_type;
        """,
        (asset_id, doc_id, file_url, provider, public_id, mime_type),
    )


def process_document_file(
    file_path: Path,
    *,
    doc_id: uuid.UUID,
    source: Optional[str],
    status: str,
    embedding_model: str,
    cursor,
) -> None:
    text = file_path.read_text(encoding="utf-8").strip()
    if not text:
        print(f"[WARN] {file_path} is empty. Skipping.", file=sys.stderr)
        return

    title = file_path.stem.replace("_", " ").title()
    insert_document(cursor, doc_id=doc_id, title=title, content=text, source=source, status=status)

    raw_chunks = chunk_text(text)
    if not raw_chunks:
        print(f"[WARN] No chunks generated for {file_path}", file=sys.stderr)
        return

    embeddings = embed_chunks(raw_chunks, embedding_model)
    doc_chunks = [
        DocumentChunk(
            chunk_id=uuid.uuid4(),
            doc_id=doc_id,
            text=chunk,
            embedding=embedding,
        )
        for chunk, embedding in zip(raw_chunks, embeddings)
    ]
    insert_chunks(cursor, doc_chunks)


def process_asset_files(
    asset_paths: Iterable[Path],
    *,
    doc_id: uuid.UUID,
    upload_folder: str,
    cursor,
) -> None:
    ensure_cloudinary_config()
    for asset_path in asset_paths:
        if asset_path.suffix.lower() not in {".png", ".jpg", ".jpeg", ".gif", ".webp"}:
            continue
        print(f"  Uploading asset {asset_path.name}…")
        upload_info = cloudinary.uploader.upload(
            str(asset_path),
            folder=upload_folder,
            resource_type="image",
            use_filename=True,
            unique_filename=True,
        )
        url = upload_info.get("secure_url") or upload_info.get("url")
        if not url:
            raise RuntimeError(f"Upload failed for {asset_path}")

        insert_asset(
            cursor,
            doc_id=doc_id,
            file_url=url,
            provider="cloudinary",
            public_id=upload_info.get("public_id"),
            mime_type=upload_info.get("resource_type"),
        )


def main() -> None:
    parser = argparse.ArgumentParser(description="Ingest medical documents into PostgreSQL with embeddings.")
    parser.add_argument("--docs-path", required=True, help="Folder containing .md/.txt files to ingest.")
    parser.add_argument("--source", default=None, help="Source label (e.g., guideline:vn-2024).")
    parser.add_argument("--status", default="active", help="Medical document status (default: active).")
    parser.add_argument("--dsn", default=os.getenv("DATABASE_URL"), help="PostgreSQL DSN.")
    parser.add_argument("--embedding-model", default="models/text-embedding-004", help="Gemini embedding model.")
    parser.add_argument("--upload-assets", action="store_true", help="Upload image assets located next to each document.")
    parser.add_argument("--assets-folder", default="medical-documents", help="Cloudinary folder when uploading assets.")
    args = parser.parse_args()

    if not args.dsn:
        parser.error("Database DSN not provided. Use --dsn or set DATABASE_URL.")
    if genai is None:
        parser.error("google-generativeai package not installed. Run `pip install google-generativeai`.")
    if not os.getenv("GEMINI_API_KEY"):
        parser.error("GEMINI_API_KEY environment variable is required.")

    docs_dir = Path(args.docs_path)
    if not docs_dir.is_dir():
        parser.error(f"{docs_dir} is not a valid directory.")

    genai.configure(api_key=os.environ["GEMINI_API_KEY"])
    embedding_model = args.embedding_model

    with psycopg2.connect(args.dsn) as conn:
        with conn.cursor() as cursor:
            for file_path in sorted(docs_dir.glob("**/*")):
                if file_path.suffix.lower() not in {".md", ".txt"}:
                    continue

                print(f"Ingesting document: {file_path}")
                doc_id = uuid.uuid4()
                process_document_file(
                    file_path,
                    doc_id=doc_id,
                    source=args.source,
                    status=args.status,
                    embedding_model=embedding_model,
                    cursor=cursor,
                )

                if args.upload_assets:
                    matching_assets = list(file_path.parent.glob(f"{file_path.stem}*.*"))
                    asset_candidates = [p for p in matching_assets if p.suffix.lower() in {".png", ".jpg", ".jpeg", ".gif", ".webp"}]
                    if asset_candidates:
                        process_asset_files(
                            asset_candidates,
                            doc_id=doc_id,
                            upload_folder=args.assets_folder,
                            cursor=cursor,
                        )

            conn.commit()
    print("Ingestion completed successfully.")


if __name__ == "__main__":
    main()
