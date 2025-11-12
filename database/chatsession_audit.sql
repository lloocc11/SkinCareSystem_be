BEGIN;

-- Drop legacy 'status' if exists
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema='public' AND table_name='ChatSessions' AND column_name='status'
    ) THEN
        EXECUTE 'ALTER TABLE "ChatSessions" DROP COLUMN "status";';
    END IF;
END;
$$;

-- Add channel with CHECK ('ai'|'specialist'|'ai_admin'), default 'ai'
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema='public' AND table_name='ChatSessions' AND column_name='channel'
    ) THEN
        EXECUTE 'ALTER TABLE "ChatSessions"
                 ADD COLUMN "channel" varchar NOT NULL DEFAULT ''ai'',
                 ADD CONSTRAINT "ck_chatsessions_channel"
                 CHECK ("channel" IN (''ai'',''specialist'',''ai_admin''));';
    END IF;
END;
$$;

-- Add specialist_id, assigned_at, closed_at
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema='public' AND table_name='ChatSessions' AND column_name='specialist_id'
    ) THEN
        EXECUTE 'ALTER TABLE "ChatSessions" ADD COLUMN "specialist_id" uuid NULL;';
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.table_constraints
            WHERE table_schema='public' AND table_name='ChatSessions' AND constraint_name='fk_chatsessions_specialist'
        ) THEN
            EXECUTE 'ALTER TABLE "ChatSessions"
                     ADD CONSTRAINT "fk_chatsessions_specialist"
                     FOREIGN KEY ("specialist_id") REFERENCES "Users"("user_id") ON DELETE SET NULL;';
        END IF;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema='public' AND table_name='ChatSessions' AND column_name='assigned_at'
    ) THEN
        EXECUTE 'ALTER TABLE "ChatSessions" ADD COLUMN "assigned_at" timestamp NULL;';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema='public' AND table_name='ChatSessions' AND column_name='closed_at'
    ) THEN
        EXECUTE 'ALTER TABLE "ChatSessions" ADD COLUMN "closed_at" timestamp NULL;';
    END IF;
END;
$$;

-- Helpful indexes
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace
        WHERE c.relname='idx_chatsessions_channel' AND n.nspname='public'
    ) THEN
        EXECUTE 'CREATE INDEX idx_chatsessions_channel ON "ChatSessions"("channel");';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace
        WHERE c.relname='idx_chatsessions_specialist_id' AND n.nspname='public'
    ) THEN
        EXECUTE 'CREATE INDEX idx_chatsessions_specialist_id ON "ChatSessions"("specialist_id");';
    END IF;
END;
$$;

COMMENT ON COLUMN "ChatSessions"."channel" IS 'ai: user↔AI (templates-only); specialist: user↔specialist; ai_admin: admin/specialist↔AI (builder, có thể tạo routine mới).';
COMMENT ON COLUMN "ChatSessions"."specialist_id" IS 'User_id của specialist đang phụ trách phiên (NULL nếu chưa gán/không áp dụng).';
COMMENT ON COLUMN "ChatSessions"."assigned_at" IS 'Thời điểm assign specialist (áp dụng cho channel=specialist).';
COMMENT ON COLUMN "ChatSessions"."closed_at" IS 'Thời điểm đóng phiên (tuỳ workflow).';

COMMIT;
