-- ============================================================
-- SkinCareSystem.sql
-- PostgreSQL schema for Skin Care System (RAG-enabled)
-- All DDL/DML statements are idempotent; safe to rerun.
-- ============================================================

SET client_min_messages TO WARNING;

BEGIN;

-- ------------------------------------------------------------
-- Extensions
-- ------------------------------------------------------------
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS vector;

-- ------------------------------------------------------------
-- Drop existing objects (reverse dependency order)
-- ------------------------------------------------------------
DROP TABLE IF EXISTS "ChatMessages" CASCADE;
DROP TABLE IF EXISTS "ChatSessions" CASCADE;
DROP TABLE IF EXISTS "ConsentRecords" CASCADE;
DROP TABLE IF EXISTS "RuleConditions" CASCADE;
DROP TABLE IF EXISTS "Rules" CASCADE;
DROP TABLE IF EXISTS "Questions" CASCADE;
DROP TABLE IF EXISTS "Symptoms" CASCADE;
DROP TABLE IF EXISTS "RoutineProgress" CASCADE;
DROP TABLE IF EXISTS "RoutineInstances" CASCADE;
DROP TABLE IF EXISTS "Feedback" CASCADE;
DROP TABLE IF EXISTS "RoutineSteps" CASCADE;
DROP TABLE IF EXISTS "Routines" CASCADE;
DROP TABLE IF EXISTS "AIResponses" CASCADE;
DROP TABLE IF EXISTS "QueryMatches" CASCADE;
DROP TABLE IF EXISTS "UserQueries" CASCADE;
DROP TABLE IF EXISTS "DocumentChunks" CASCADE;
DROP TABLE IF EXISTS "MedicalDocumentAssets" CASCADE;
DROP TABLE IF EXISTS "MedicalDocuments" CASCADE;
DROP TABLE IF EXISTS "AIAnalysis" CASCADE;
DROP TABLE IF EXISTS "UserSymptoms" CASCADE;
DROP TABLE IF EXISTS "UserAnswers" CASCADE;
DROP TABLE IF EXISTS "Users" CASCADE;
DROP TABLE IF EXISTS "Roles" CASCADE;

-- ------------------------------------------------------------
-- Trigger function
-- ------------------------------------------------------------
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ------------------------------------------------------------
-- Tables
-- ------------------------------------------------------------
CREATE TABLE "Roles" (
  "role_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "name" varchar UNIQUE NOT NULL,
  "description" text,
  "status" varchar NOT NULL CHECK (status IN ('active', 'inactive')),
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "Users" (
  "user_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "full_name" varchar NOT NULL,
  "email" varchar UNIQUE NOT NULL,
  "google_id" varchar UNIQUE NOT NULL,
  "role_id" uuid NOT NULL,
  "skin_type" varchar,
  "date_of_birth" date,
  "status" varchar NOT NULL CHECK (status IN ('active', 'inactive', 'banned')),
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("role_id") REFERENCES "Roles" ("role_id") ON DELETE RESTRICT
);

CREATE TABLE "UserAnswers" (
  "answer_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "user_id" uuid NOT NULL,
  "question_id" uuid NOT NULL,
  "answer_value" text NOT NULL,
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("user_id") REFERENCES "Users" ("user_id") ON DELETE CASCADE
);

CREATE TABLE "UserSymptoms" (
  "user_symptom_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "user_id" uuid NOT NULL,
  "symptom_id" uuid NOT NULL,
  "reported_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("user_id") REFERENCES "Users" ("user_id") ON DELETE CASCADE
);

CREATE TABLE "AIAnalysis" (
  "analysis_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "user_id" uuid NOT NULL,
  "chat_message_id" uuid NOT NULL,
  "raw_input" text NOT NULL,
  "result" jsonb NOT NULL,
  "confidence" float NOT NULL CHECK (confidence >= 0 AND confidence <= 1),
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("user_id") REFERENCES "Users" ("user_id") ON DELETE CASCADE
);

CREATE TABLE "MedicalDocuments" (
  "doc_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "title" varchar NOT NULL,
  "content" text NOT NULL,
  "source" varchar,
  "status" varchar NOT NULL CHECK (status IN ('active', 'inactive')),
  "last_updated" timestamp,
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "MedicalDocumentAssets" (
  "asset_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "doc_id" uuid NOT NULL,
  "file_url" text NOT NULL,
  "public_id" varchar,
  "provider" varchar DEFAULT 'cloudinary',
  "mime_type" varchar,
  "size_bytes" integer,
  "width" integer,
  "height" integer,
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("doc_id") REFERENCES "MedicalDocuments" ("doc_id") ON DELETE CASCADE
);

CREATE TABLE "DocumentChunks" (
  "chunk_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "doc_id" uuid NOT NULL,
  "chunk_text" text NOT NULL,
  "embedding" vector(1536),
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("doc_id") REFERENCES "MedicalDocuments" ("doc_id") ON DELETE CASCADE
);

CREATE TABLE "UserQueries" (
  "query_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "user_id" uuid NOT NULL,
  "query_text" text NOT NULL,
  "query_embedding" vector(1536),
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("user_id") REFERENCES "Users" ("user_id") ON DELETE CASCADE
);

CREATE TABLE "QueryMatches" (
  "match_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "query_id" uuid NOT NULL,
  "chunk_id" uuid NOT NULL,
  "similarity_score" float NOT NULL CHECK (similarity_score >= 0 AND similarity_score <= 1),
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("query_id") REFERENCES "UserQueries" ("query_id") ON DELETE CASCADE,
  FOREIGN KEY ("chunk_id") REFERENCES "DocumentChunks" ("chunk_id") ON DELETE CASCADE
);

CREATE TABLE "AIResponses" (
  "response_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "query_id" uuid NOT NULL,
  "response_text" text NOT NULL,
  "response_type" varchar NOT NULL CHECK (response_type IN ('recommendation', 'explanation', 'disclaimer')),
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("query_id") REFERENCES "UserQueries" ("query_id") ON DELETE CASCADE
);

CREATE TABLE "Routines" (
  "routine_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "user_id" uuid NOT NULL,
  "analysis_id" uuid,
  "description" text,
  "version" int DEFAULT 1 CHECK (version >= 1),
  "parent_routine_id" uuid,
  "status" varchar NOT NULL CHECK (status IN ('active', 'archived')),
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("user_id") REFERENCES "Users" ("user_id") ON DELETE CASCADE,
  FOREIGN KEY ("parent_routine_id") REFERENCES "Routines" ("routine_id") ON DELETE SET NULL
);

CREATE TABLE "RoutineSteps" (
  "step_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "routine_id" uuid NOT NULL,
  "step_order" int NOT NULL,
  "instruction" text NOT NULL,
  "time_of_day" varchar NOT NULL CHECK (time_of_day IN ('morning', 'evening', 'both')),
  "frequency" varchar NOT NULL DEFAULT 'daily' CHECK (frequency IN ('daily', 'twice_daily', 'weekly', 'as_needed')),
  FOREIGN KEY ("routine_id") REFERENCES "Routines" ("routine_id") ON DELETE CASCADE
);

CREATE TABLE "Feedback" (
  "feedback_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "routine_id" uuid NOT NULL,
  "step_id" uuid,
  "user_id" uuid NOT NULL,
  "rating" int NOT NULL CHECK (rating BETWEEN 1 AND 5),
  "comment" text,
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("routine_id") REFERENCES "Routines" ("routine_id") ON DELETE CASCADE,
  FOREIGN KEY ("step_id") REFERENCES "RoutineSteps" ("step_id") ON DELETE CASCADE,
  FOREIGN KEY ("user_id") REFERENCES "Users" ("user_id") ON DELETE CASCADE
);

CREATE TABLE "RoutineInstances" (
  "instance_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "routine_id" uuid NOT NULL,
  "user_id" uuid NOT NULL,
  "start_date" date NOT NULL,
  "end_date" date,
  "status" varchar NOT NULL CHECK (status IN ('active', 'paused', 'completed')),
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("routine_id") REFERENCES "Routines" ("routine_id") ON DELETE CASCADE,
  FOREIGN KEY ("user_id") REFERENCES "Users" ("user_id") ON DELETE CASCADE
);

CREATE TABLE "RoutineProgress" (
  "progress_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "instance_id" uuid NOT NULL,
  "step_id" uuid NOT NULL,
  "completed_at" timestamp NOT NULL,
  "photo_url" varchar,
  "note" text,
  "status" varchar NOT NULL DEFAULT 'completed' CHECK (status IN ('completed', 'skipped', 'pending')),
  FOREIGN KEY ("instance_id") REFERENCES "RoutineInstances" ("instance_id") ON DELETE CASCADE,
  FOREIGN KEY ("step_id") REFERENCES "RoutineSteps" ("step_id") ON DELETE CASCADE
);

CREATE TABLE "Symptoms" (
  "symptom_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "name" varchar UNIQUE NOT NULL,
  "description" text,
  "example_image_url" varchar,
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "Questions" (
  "question_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "text" varchar NOT NULL,
  "type" varchar NOT NULL CHECK (type IN ('choice', 'multi-choice', 'text')),
  "options" jsonb,
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "Rules" (
  "rule_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "recommendation" text NOT NULL,
  "urgency_level" varchar NOT NULL CHECK (urgency_level IN ('normal', 'caution', 'urgent')),
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "RuleConditions" (
  "rule_condition_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "rule_id" uuid NOT NULL,
  "symptom_id" uuid,
  "question_id" uuid,
  "operator" varchar NOT NULL CHECK (operator IN ('=', '>', '<', 'IN', 'NOT IN')),
  "value" text,
  FOREIGN KEY ("rule_id") REFERENCES "Rules" ("rule_id") ON DELETE CASCADE,
  FOREIGN KEY ("symptom_id") REFERENCES "Symptoms" ("symptom_id") ON DELETE CASCADE,
  FOREIGN KEY ("question_id") REFERENCES "Questions" ("question_id") ON DELETE CASCADE,
  CHECK (symptom_id IS NOT NULL OR question_id IS NOT NULL)
);

CREATE TABLE "ConsentRecords" (
  "consent_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "user_id" uuid NOT NULL,
  "consent_type" varchar NOT NULL,
  "consent_text" text NOT NULL,
  "given" boolean NOT NULL,
  "given_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("user_id") REFERENCES "Users" ("user_id") ON DELETE CASCADE
);

CREATE TABLE "ChatSessions" (
  "session_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "user_id" uuid NOT NULL,
  "title" varchar,
  "status" varchar DEFAULT 'active',
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("user_id") REFERENCES "Users" ("user_id") ON DELETE CASCADE
);

CREATE TABLE "ChatMessages" (
  "message_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  "session_id" uuid NOT NULL,
  "user_id" uuid NOT NULL,
  "content" text,
  "image_url" varchar,
  "message_type" varchar NOT NULL CHECK (message_type IN ('text', 'image', 'mixed')),
  "role" varchar NOT NULL CHECK (role IN ('user', 'assistant')),
  "created_at" timestamp DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY ("session_id") REFERENCES "ChatSessions" ("session_id") ON DELETE CASCADE,
  FOREIGN KEY ("user_id") REFERENCES "Users" ("user_id") ON DELETE CASCADE
);

-- ------------------------------------------------------------
-- Deferred foreign keys
-- ------------------------------------------------------------
ALTER TABLE "AIAnalysis"
    ADD FOREIGN KEY ("chat_message_id") REFERENCES "ChatMessages" ("message_id") ON DELETE CASCADE;
ALTER TABLE "UserAnswers"
    ADD FOREIGN KEY ("question_id") REFERENCES "Questions" ("question_id") ON DELETE CASCADE;
ALTER TABLE "UserSymptoms"
    ADD FOREIGN KEY ("symptom_id") REFERENCES "Symptoms" ("symptom_id") ON DELETE CASCADE;
ALTER TABLE "Routines"
    ADD FOREIGN KEY ("analysis_id") REFERENCES "AIAnalysis" ("analysis_id") ON DELETE SET NULL;

-- ------------------------------------------------------------
-- Indexes (idempotent)
-- ------------------------------------------------------------
CREATE INDEX IF NOT EXISTS "idx_roles_role_id" ON "Roles" USING BTREE ("role_id");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_roles_name" ON "Roles" USING BTREE ("name");

CREATE INDEX IF NOT EXISTS "idx_users_user_id" ON "Users" USING BTREE ("user_id");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_users_email" ON "Users" USING BTREE ("email");
CREATE INDEX IF NOT EXISTS "idx_users_role_id" ON "Users" USING BTREE ("role_id");

CREATE INDEX IF NOT EXISTS "idx_useranswers_answer_id" ON "UserAnswers" USING BTREE ("answer_id");
CREATE INDEX IF NOT EXISTS "idx_useranswers_user_id" ON "UserAnswers" USING BTREE ("user_id");
CREATE INDEX IF NOT EXISTS "idx_useranswers_question_id" ON "UserAnswers" USING BTREE ("question_id");

CREATE INDEX IF NOT EXISTS "idx_usersymptoms_user_symptom_id" ON "UserSymptoms" USING BTREE ("user_symptom_id");
CREATE INDEX IF NOT EXISTS "idx_usersymptoms_user_id" ON "UserSymptoms" USING BTREE ("user_id");
CREATE INDEX IF NOT EXISTS "idx_usersymptoms_symptom_id" ON "UserSymptoms" USING BTREE ("symptom_id");

CREATE INDEX IF NOT EXISTS "idx_aianalysis_analysis_id" ON "AIAnalysis" USING BTREE ("analysis_id");
CREATE INDEX IF NOT EXISTS "idx_aianalysis_user_id" ON "AIAnalysis" USING BTREE ("user_id");
CREATE INDEX IF NOT EXISTS "idx_aianalysis_chat_message_id" ON "AIAnalysis" USING BTREE ("chat_message_id");

CREATE INDEX IF NOT EXISTS "idx_medicaldocuments_doc_id" ON "MedicalDocuments" USING BTREE ("doc_id");
CREATE INDEX IF NOT EXISTS "idx_medicaldocuments_created_at" ON "MedicalDocuments" USING BTREE ("created_at");

CREATE INDEX IF NOT EXISTS "idx_medicaldocumentassets_asset_id" ON "MedicalDocumentAssets" USING BTREE ("asset_id");
CREATE INDEX IF NOT EXISTS "idx_medicaldocumentassets_doc_id" ON "MedicalDocumentAssets" USING BTREE ("doc_id");
CREATE INDEX IF NOT EXISTS "idx_medicaldocumentassets_public_id" ON "MedicalDocumentAssets" USING BTREE ("public_id");

CREATE INDEX IF NOT EXISTS "idx_documentchunks_chunk_id" ON "DocumentChunks" USING BTREE ("chunk_id");
CREATE INDEX IF NOT EXISTS "idx_documentchunks_doc_id" ON "DocumentChunks" USING BTREE ("doc_id");
CREATE INDEX IF NOT EXISTS "idx_documentchunks_created_at" ON "DocumentChunks" USING BTREE ("created_at");

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_class c
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE c.relname = 'idx_documentchunks_embedding'
          AND n.nspname = 'public'
    ) THEN
        EXECUTE 'CREATE INDEX idx_documentchunks_embedding ON "DocumentChunks" USING HNSW ("embedding" vector_cosine_ops);';
    END IF;
END;
$$;

CREATE INDEX IF NOT EXISTS "idx_userqueries_query_id" ON "UserQueries" USING BTREE ("query_id");
CREATE INDEX IF NOT EXISTS "idx_userqueries_user_id" ON "UserQueries" USING BTREE ("user_id");
CREATE INDEX IF NOT EXISTS "idx_userqueries_created_at" ON "UserQueries" USING BTREE ("created_at");

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_class c
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE c.relname = 'idx_userqueries_query_embedding'
          AND n.nspname = 'public'
    ) THEN
        EXECUTE 'CREATE INDEX idx_userqueries_query_embedding ON "UserQueries" USING HNSW ("query_embedding" vector_cosine_ops);';
    END IF;
END;
$$;

CREATE INDEX IF NOT EXISTS "idx_querymatches_match_id" ON "QueryMatches" USING BTREE ("match_id");
CREATE INDEX IF NOT EXISTS "idx_querymatches_query_id" ON "QueryMatches" USING BTREE ("query_id");
CREATE INDEX IF NOT EXISTS "idx_querymatches_chunk_id" ON "QueryMatches" USING BTREE ("chunk_id");

CREATE INDEX IF NOT EXISTS "idx_airesponses_response_id" ON "AIResponses" USING BTREE ("response_id");
CREATE INDEX IF NOT EXISTS "idx_airesponses_query_id" ON "AIResponses" USING BTREE ("query_id");

CREATE INDEX IF NOT EXISTS "idx_routines_routine_id" ON "Routines" USING BTREE ("routine_id");
CREATE INDEX IF NOT EXISTS "idx_routines_user_id" ON "Routines" USING BTREE ("user_id");
CREATE INDEX IF NOT EXISTS "idx_routines_analysis_id" ON "Routines" USING BTREE ("analysis_id");

CREATE INDEX IF NOT EXISTS "idx_routinesteps_step_id" ON "RoutineSteps" USING BTREE ("step_id");
CREATE INDEX IF NOT EXISTS "idx_routinesteps_routine_id" ON "RoutineSteps" USING BTREE ("routine_id");

CREATE INDEX IF NOT EXISTS "idx_feedback_feedback_id" ON "Feedback" USING BTREE ("feedback_id");
CREATE INDEX IF NOT EXISTS "idx_feedback_routine_id" ON "Feedback" USING BTREE ("routine_id");
CREATE INDEX IF NOT EXISTS "idx_feedback_step_id" ON "Feedback" USING BTREE ("step_id");
CREATE INDEX IF NOT EXISTS "idx_feedback_user_id" ON "Feedback" USING BTREE ("user_id");

CREATE INDEX IF NOT EXISTS "idx_routineinstances_instance_id" ON "RoutineInstances" USING BTREE ("instance_id");
CREATE INDEX IF NOT EXISTS "idx_routineinstances_routine_id" ON "RoutineInstances" USING BTREE ("routine_id");
CREATE INDEX IF NOT EXISTS "idx_routineinstances_user_id" ON "RoutineInstances" USING BTREE ("user_id");

CREATE INDEX IF NOT EXISTS "idx_routineprogress_progress_id" ON "RoutineProgress" USING BTREE ("progress_id");
CREATE INDEX IF NOT EXISTS "idx_routineprogress_instance_id" ON "RoutineProgress" USING BTREE ("instance_id");
CREATE INDEX IF NOT EXISTS "idx_routineprogress_step_id" ON "RoutineProgress" USING BTREE ("step_id");

CREATE INDEX IF NOT EXISTS "idx_symptoms_symptom_id" ON "Symptoms" USING BTREE ("symptom_id");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_symptoms_name" ON "Symptoms" USING BTREE ("name");

CREATE INDEX IF NOT EXISTS "idx_questions_question_id" ON "Questions" USING BTREE ("question_id");

CREATE INDEX IF NOT EXISTS "idx_rules_rule_id" ON "Rules" USING BTREE ("rule_id");

CREATE INDEX IF NOT EXISTS "idx_ruleconditions_rule_condition_id" ON "RuleConditions" USING BTREE ("rule_condition_id");
CREATE INDEX IF NOT EXISTS "idx_ruleconditions_rule_id" ON "RuleConditions" USING BTREE ("rule_id");
CREATE INDEX IF NOT EXISTS "idx_ruleconditions_symptom_id" ON "RuleConditions" USING BTREE ("symptom_id");
CREATE INDEX IF NOT EXISTS "idx_ruleconditions_question_id" ON "RuleConditions" USING BTREE ("question_id");

CREATE INDEX IF NOT EXISTS "idx_consentrecords_consent_id" ON "ConsentRecords" USING BTREE ("consent_id");
CREATE INDEX IF NOT EXISTS "idx_consentrecords_user_id" ON "ConsentRecords" USING BTREE ("user_id");

CREATE INDEX IF NOT EXISTS "idx_chatsessions_session_id" ON "ChatSessions" USING BTREE ("session_id");
CREATE INDEX IF NOT EXISTS "idx_chatsessions_user_id" ON "ChatSessions" USING BTREE ("user_id");

CREATE INDEX IF NOT EXISTS "idx_chatmessages_message_id" ON "ChatMessages" USING BTREE ("message_id");
CREATE INDEX IF NOT EXISTS "idx_chatmessages_session_id" ON "ChatMessages" USING BTREE ("session_id");
CREATE INDEX IF NOT EXISTS "idx_chatmessages_user_id" ON "ChatMessages" USING BTREE ("user_id");

-- ------------------------------------------------------------
-- Triggers (idempotent)
-- ------------------------------------------------------------
DROP TRIGGER IF EXISTS update_roles_updated_at ON "Roles";
CREATE TRIGGER update_roles_updated_at BEFORE UPDATE ON "Roles"
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_users_updated_at ON "Users";
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON "Users"
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_useranswers_updated_at ON "UserAnswers";
CREATE TRIGGER update_useranswers_updated_at BEFORE UPDATE ON "UserAnswers"
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_aianalysis_updated_at ON "AIAnalysis";
CREATE TRIGGER update_aianalysis_updated_at BEFORE UPDATE ON "AIAnalysis"
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_routines_updated_at ON "Routines";
CREATE TRIGGER update_routines_updated_at BEFORE UPDATE ON "Routines"
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_chatsessions_updated_at ON "ChatSessions";
CREATE TRIGGER update_chatsessions_updated_at BEFORE UPDATE ON "ChatSessions"
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ------------------------------------------------------------
-- Comments on database, tables, and columns
-- ------------------------------------------------------------
DO $$
DECLARE
    dbname text := current_database();
BEGIN
    EXECUTE format('COMMENT ON DATABASE %I IS %L', dbname, 'Cơ sở dữ liệu SkinCareSystem - đã thiết lập đầy đủ schema và seed mẫu.');
END;
$$;

COMMENT ON TABLE "Roles" IS 'Danh mục vai trò người dùng.';
COMMENT ON COLUMN "Roles"."role_id" IS 'Khóa chính vai trò.';
COMMENT ON COLUMN "Roles"."name" IS 'Tên vai trò duy nhất.';
COMMENT ON COLUMN "Roles"."description" IS 'Mô tả vai trò.';
COMMENT ON COLUMN "Roles"."status" IS 'Trạng thái hoạt động của vai trò.';
COMMENT ON COLUMN "Roles"."created_at" IS 'Thời điểm tạo vai trò.';
COMMENT ON COLUMN "Roles"."updated_at" IS 'Thời điểm cập nhật gần nhất.';

COMMENT ON TABLE "Users" IS 'Thông tin người dùng hệ thống.';
COMMENT ON COLUMN "Users"."user_id" IS 'Khóa chính người dùng.';
COMMENT ON COLUMN "Users"."full_name" IS 'Họ và tên đầy đủ.';
COMMENT ON COLUMN "Users"."email" IS 'Email đăng nhập duy nhất.';
COMMENT ON COLUMN "Users"."google_id" IS 'ID Google dùng để xác thực.';
COMMENT ON COLUMN "Users"."role_id" IS 'Vai trò của người dùng.';
COMMENT ON COLUMN "Users"."skin_type" IS 'Loại da của người dùng.';
COMMENT ON COLUMN "Users"."date_of_birth" IS 'Ngày sinh của người dùng.';
COMMENT ON COLUMN "Users"."status" IS 'Trạng thái người dùng (active/inactive/banned).';
COMMENT ON COLUMN "Users"."created_at" IS 'Thời điểm tạo người dùng.';
COMMENT ON COLUMN "Users"."updated_at" IS 'Thời điểm cập nhật người dùng.';

COMMENT ON TABLE "UserAnswers" IS 'Câu trả lời khảo sát của người dùng.';
COMMENT ON COLUMN "UserAnswers"."answer_id" IS 'Khóa chính câu trả lời.';
COMMENT ON COLUMN "UserAnswers"."user_id" IS 'Người dùng thực hiện trả lời.';
COMMENT ON COLUMN "UserAnswers"."question_id" IS 'Câu hỏi được trả lời.';
COMMENT ON COLUMN "UserAnswers"."answer_value" IS 'Giá trị câu trả lời.';
COMMENT ON COLUMN "UserAnswers"."created_at" IS 'Thời điểm tạo câu trả lời.';
COMMENT ON COLUMN "UserAnswers"."updated_at" IS 'Thời điểm cập nhật câu trả lời.';

COMMENT ON TABLE "UserSymptoms" IS 'Ghi nhận triệu chứng người dùng gặp phải.';
COMMENT ON COLUMN "UserSymptoms"."user_symptom_id" IS 'Khóa chính bản ghi triệu chứng.';
COMMENT ON COLUMN "UserSymptoms"."user_id" IS 'Người dùng liên quan.';
COMMENT ON COLUMN "UserSymptoms"."symptom_id" IS 'Triệu chứng đã chọn.';
COMMENT ON COLUMN "UserSymptoms"."reported_at" IS 'Thời điểm báo cáo triệu chứng.';

COMMENT ON TABLE "AIAnalysis" IS 'Kết quả phân tích AI cho từng tin nhắn.';
COMMENT ON COLUMN "AIAnalysis"."analysis_id" IS 'Khóa chính bản ghi phân tích.';
COMMENT ON COLUMN "AIAnalysis"."user_id" IS 'Người dùng được phân tích.';
COMMENT ON COLUMN "AIAnalysis"."chat_message_id" IS 'Tin nhắn nguồn được phân tích.';
COMMENT ON COLUMN "AIAnalysis"."raw_input" IS 'Dữ liệu đầu vào gốc.';
COMMENT ON COLUMN "AIAnalysis"."result" IS 'Kết quả phân tích dạng JSON.';
COMMENT ON COLUMN "AIAnalysis"."confidence" IS 'Độ tin cậy của phân tích (0-1).';
COMMENT ON COLUMN "AIAnalysis"."created_at" IS 'Thời điểm tạo phân tích.';
COMMENT ON COLUMN "AIAnalysis"."updated_at" IS 'Thời điểm cập nhật phân tích.';

COMMENT ON TABLE "MedicalDocuments" IS 'Tài liệu tri thức y khoa phục vụ RAG.';
COMMENT ON COLUMN "MedicalDocuments"."doc_id" IS 'Khóa chính tài liệu.';
COMMENT ON COLUMN "MedicalDocuments"."title" IS 'Tiêu đề tài liệu.';
COMMENT ON COLUMN "MedicalDocuments"."content" IS 'Nội dung tài liệu.';
COMMENT ON COLUMN "MedicalDocuments"."source" IS 'Nguồn trích dẫn tài liệu.';
COMMENT ON COLUMN "MedicalDocuments"."status" IS 'Trạng thái hiển thị tài liệu.';
COMMENT ON COLUMN "MedicalDocuments"."last_updated" IS 'Thời điểm cập nhật nội dung gần nhất.';
COMMENT ON COLUMN "MedicalDocuments"."created_at" IS 'Thời điểm tạo tài liệu.';

COMMENT ON TABLE "MedicalDocumentAssets" IS 'Tập hợp ảnh/tệp đính kèm cho tài liệu y khoa.';
COMMENT ON COLUMN "MedicalDocumentAssets"."asset_id" IS 'Khóa chính bản ghi tài nguyên.';
COMMENT ON COLUMN "MedicalDocumentAssets"."doc_id" IS 'Tài liệu y khoa sở hữu ảnh.';
COMMENT ON COLUMN "MedicalDocumentAssets"."file_url" IS 'URL công khai của ảnh/tệp.';
COMMENT ON COLUMN "MedicalDocumentAssets"."public_id" IS 'Public ID từ Cloudinary (hoặc provider).';
COMMENT ON COLUMN "MedicalDocumentAssets"."provider" IS 'Tên nhà cung cấp lưu trữ (mặc định Cloudinary).';
COMMENT ON COLUMN "MedicalDocumentAssets"."mime_type" IS 'Loại MIME của tài nguyên.';
COMMENT ON COLUMN "MedicalDocumentAssets"."size_bytes" IS 'Dung lượng tệp (byte).';
COMMENT ON COLUMN "MedicalDocumentAssets"."width" IS 'Chiều rộng ảnh (pixel).';
COMMENT ON COLUMN "MedicalDocumentAssets"."height" IS 'Chiều cao ảnh (pixel).';
COMMENT ON COLUMN "MedicalDocumentAssets"."created_at" IS 'Thời điểm lưu tài nguyên.';

COMMENT ON TABLE "DocumentChunks" IS 'Các đoạn văn bản đã chia nhỏ từ tài liệu.';
COMMENT ON COLUMN "DocumentChunks"."chunk_id" IS 'Khóa chính đoạn văn.';
COMMENT ON COLUMN "DocumentChunks"."doc_id" IS 'Tài liệu gốc chứa đoạn văn.';
COMMENT ON COLUMN "DocumentChunks"."chunk_text" IS 'Nội dung đoạn.';
COMMENT ON COLUMN "DocumentChunks"."embedding" IS 'Vector(1536) dùng cho tìm kiếm ngữ nghĩa.';
COMMENT ON COLUMN "DocumentChunks"."created_at" IS 'Thời điểm tạo đoạn.';

COMMENT ON TABLE "UserQueries" IS 'Lịch sử truy vấn của người dùng.';
COMMENT ON COLUMN "UserQueries"."query_id" IS 'Khóa chính truy vấn.';
COMMENT ON COLUMN "UserQueries"."user_id" IS 'Người dùng thực hiện truy vấn.';
COMMENT ON COLUMN "UserQueries"."query_text" IS 'Nội dung câu hỏi.';
COMMENT ON COLUMN "UserQueries"."query_embedding" IS 'Vector(1536) biểu diễn truy vấn.';
COMMENT ON COLUMN "UserQueries"."created_at" IS 'Thời điểm tạo truy vấn.';

COMMENT ON TABLE "QueryMatches" IS 'Kết quả đối sánh đoạn văn cho truy vấn.';
COMMENT ON COLUMN "QueryMatches"."match_id" IS 'Khóa chính bản ghi đối sánh.';
COMMENT ON COLUMN "QueryMatches"."query_id" IS 'Truy vấn liên quan.';
COMMENT ON COLUMN "QueryMatches"."chunk_id" IS 'Đoạn văn được đối sánh.';
COMMENT ON COLUMN "QueryMatches"."similarity_score" IS 'Điểm tương đồng (0-1).';
COMMENT ON COLUMN "QueryMatches"."created_at" IS 'Thời điểm tạo bản ghi.';

COMMENT ON TABLE "AIResponses" IS 'Phản hồi AI sau truy vấn người dùng.';
COMMENT ON COLUMN "AIResponses"."response_id" IS 'Khóa chính phản hồi.';
COMMENT ON COLUMN "AIResponses"."query_id" IS 'Truy vấn nguồn.';
COMMENT ON COLUMN "AIResponses"."response_text" IS 'Nội dung phản hồi.';
COMMENT ON COLUMN "AIResponses"."response_type" IS 'Loại phản hồi (recommendation/explanation/disclaimer).';
COMMENT ON COLUMN "AIResponses"."created_at" IS 'Thời điểm tạo phản hồi.';

COMMENT ON TABLE "Routines" IS 'Liệu trình chăm sóc da do AI đề xuất.';
COMMENT ON COLUMN "Routines"."routine_id" IS 'Khóa chính liệu trình.';
COMMENT ON COLUMN "Routines"."user_id" IS 'Người dùng sở hữu liệu trình.';
COMMENT ON COLUMN "Routines"."analysis_id" IS 'Phân tích AI liên quan.';
COMMENT ON COLUMN "Routines"."description" IS 'Mô tả chung liệu trình.';
COMMENT ON COLUMN "Routines"."version" IS 'Phiên bản liệu trình.';
COMMENT ON COLUMN "Routines"."parent_routine_id" IS 'Liệu trình cha (nếu có).';
COMMENT ON COLUMN "Routines"."status" IS 'Trạng thái liệu trình.';
COMMENT ON COLUMN "Routines"."created_at" IS 'Thời điểm tạo liệu trình.';
COMMENT ON COLUMN "Routines"."updated_at" IS 'Thời điểm cập nhật liệu trình.';

COMMENT ON TABLE "RoutineSteps" IS 'Các bước cụ thể trong liệu trình.';
COMMENT ON COLUMN "RoutineSteps"."step_id" IS 'Khóa chính bước liệu trình.';
COMMENT ON COLUMN "RoutineSteps"."routine_id" IS 'Liệu trình chứa bước này.';
COMMENT ON COLUMN "RoutineSteps"."step_order" IS 'Thứ tự thực hiện bước.';
COMMENT ON COLUMN "RoutineSteps"."instruction" IS 'Hướng dẫn chi tiết.';
COMMENT ON COLUMN "RoutineSteps"."time_of_day" IS 'Thời điểm trong ngày.';
COMMENT ON COLUMN "RoutineSteps"."frequency" IS 'Tần suất thực hiện.'; 

COMMENT ON TABLE "Feedback" IS 'Phản hồi của người dùng về liệu trình/bước.';
COMMENT ON COLUMN "Feedback"."feedback_id" IS 'Khóa chính phản hồi.';
COMMENT ON COLUMN "Feedback"."routine_id" IS 'Liệu trình được phản hồi.';
COMMENT ON COLUMN "Feedback"."step_id" IS 'Bước liên quan (nếu có).';
COMMENT ON COLUMN "Feedback"."user_id" IS 'Người dùng phản hồi.';
COMMENT ON COLUMN "Feedback"."rating" IS 'Điểm đánh giá (1-5).';
COMMENT ON COLUMN "Feedback"."comment" IS 'Nội dung phản hồi.';
COMMENT ON COLUMN "Feedback"."created_at" IS 'Thời điểm tạo phản hồi.';

COMMENT ON TABLE "RoutineInstances" IS 'Lần triển khai liệu trình theo thời gian.';
COMMENT ON COLUMN "RoutineInstances"."instance_id" IS 'Khóa chính instance.';
COMMENT ON COLUMN "RoutineInstances"."routine_id" IS 'Liệu trình triển khai.';
COMMENT ON COLUMN "RoutineInstances"."user_id" IS 'Người dùng áp dụng.';
COMMENT ON COLUMN "RoutineInstances"."start_date" IS 'Ngày bắt đầu.';
COMMENT ON COLUMN "RoutineInstances"."end_date" IS 'Ngày kết thúc (nếu có).';
COMMENT ON COLUMN "RoutineInstances"."status" IS 'Trạng thái thực hiện.';
COMMENT ON COLUMN "RoutineInstances"."created_at" IS 'Thời điểm tạo instance.';

COMMENT ON TABLE "RoutineProgress" IS 'Tiến độ thực hiện từng bước.';
COMMENT ON COLUMN "RoutineProgress"."progress_id" IS 'Khóa chính tiến độ.';
COMMENT ON COLUMN "RoutineProgress"."instance_id" IS 'Instance liên quan.';
COMMENT ON COLUMN "RoutineProgress"."step_id" IS 'Bước được ghi nhận.';
COMMENT ON COLUMN "RoutineProgress"."completed_at" IS 'Thời điểm hoàn tất.';
COMMENT ON COLUMN "RoutineProgress"."photo_url" IS 'Ảnh minh chứng (nếu có).';
COMMENT ON COLUMN "RoutineProgress"."note" IS 'Ghi chú thêm.';
COMMENT ON COLUMN "RoutineProgress"."status" IS 'Trạng thái tiến độ.';

COMMENT ON TABLE "Symptoms" IS 'Danh sách triệu chứng da liễu.';
COMMENT ON COLUMN "Symptoms"."symptom_id" IS 'Khóa chính triệu chứng.';
COMMENT ON COLUMN "Symptoms"."name" IS 'Tên triệu chứng duy nhất.';
COMMENT ON COLUMN "Symptoms"."description" IS 'Mô tả chi tiết.';
COMMENT ON COLUMN "Symptoms"."example_image_url" IS 'Ảnh minh họa.';
COMMENT ON COLUMN "Symptoms"."created_at" IS 'Thời điểm tạo triệu chứng.';

COMMENT ON TABLE "Questions" IS 'Các câu hỏi khảo sát người dùng.';
COMMENT ON COLUMN "Questions"."question_id" IS 'Khóa chính câu hỏi.';
COMMENT ON COLUMN "Questions"."text" IS 'Nội dung câu hỏi.';
COMMENT ON COLUMN "Questions"."type" IS 'Loại câu hỏi (choice/multi-choice/text).';
COMMENT ON COLUMN "Questions"."options" IS 'Danh sách lựa chọn (JSON).';
COMMENT ON COLUMN "Questions"."created_at" IS 'Thời điểm tạo câu hỏi.';

COMMENT ON TABLE "Rules" IS 'Tập luật gợi ý/skincare.';
COMMENT ON COLUMN "Rules"."rule_id" IS 'Khóa chính luật.';
COMMENT ON COLUMN "Rules"."recommendation" IS 'Khuyến nghị tương ứng.';
COMMENT ON COLUMN "Rules"."urgency_level" IS 'Mức độ khẩn cấp của luật.';
COMMENT ON COLUMN "Rules"."created_at" IS 'Thời điểm tạo luật.';

COMMENT ON TABLE "RuleConditions" IS 'Điều kiện áp dụng cho mỗi luật.';
COMMENT ON COLUMN "RuleConditions"."rule_condition_id" IS 'Khóa chính điều kiện.';
COMMENT ON COLUMN "RuleConditions"."rule_id" IS 'Luật được áp dụng.';
COMMENT ON COLUMN "RuleConditions"."symptom_id" IS 'Triệu chứng điều kiện.';
COMMENT ON COLUMN "RuleConditions"."question_id" IS 'Câu hỏi điều kiện.';
COMMENT ON COLUMN "RuleConditions"."operator" IS 'Toán tử so sánh.';
COMMENT ON COLUMN "RuleConditions"."value" IS 'Giá trị so sánh.';

COMMENT ON TABLE "ConsentRecords" IS 'Bản ghi đồng ý của người dùng.';
COMMENT ON COLUMN "ConsentRecords"."consent_id" IS 'Khóa chính đồng ý.';
COMMENT ON COLUMN "ConsentRecords"."user_id" IS 'Người dùng cho phép.';
COMMENT ON COLUMN "ConsentRecords"."consent_type" IS 'Loại đồng ý.';
COMMENT ON COLUMN "ConsentRecords"."consent_text" IS 'Nội dung đồng ý.';
COMMENT ON COLUMN "ConsentRecords"."given" IS 'Trạng thái đồng ý.';
COMMENT ON COLUMN "ConsentRecords"."given_at" IS 'Thời điểm xác nhận.';

COMMENT ON TABLE "ChatSessions" IS 'Phiên trò chuyện giữa người dùng và AI.';
COMMENT ON COLUMN "ChatSessions"."session_id" IS 'Khóa chính phiên chat.';
COMMENT ON COLUMN "ChatSessions"."user_id" IS 'Chủ sở hữu phiên chat.';
COMMENT ON COLUMN "ChatSessions"."title" IS 'Tiêu đề phiên chat.';
COMMENT ON COLUMN "ChatSessions"."status" IS 'Trạng thái phiên chat.';
COMMENT ON COLUMN "ChatSessions"."created_at" IS 'Thời điểm tạo phiên.';
COMMENT ON COLUMN "ChatSessions"."updated_at" IS 'Thời điểm cập nhật phiên.';

COMMENT ON TABLE "ChatMessages" IS 'Tin nhắn trong phiên trò chuyện.';
COMMENT ON COLUMN "ChatMessages"."message_id" IS 'Khóa chính tin nhắn.';
COMMENT ON COLUMN "ChatMessages"."session_id" IS 'Phiên chat chứa tin nhắn.';
COMMENT ON COLUMN "ChatMessages"."user_id" IS 'Người gửi (user hoặc bot).';
COMMENT ON COLUMN "ChatMessages"."content" IS 'Nội dung văn bản.';
COMMENT ON COLUMN "ChatMessages"."image_url" IS 'URL ảnh đính kèm (nếu có).';
COMMENT ON COLUMN "ChatMessages"."message_type" IS 'Loại tin nhắn (text/image/mixed).';
COMMENT ON COLUMN "ChatMessages"."role" IS 'Vai trò người gửi.';
COMMENT ON COLUMN "ChatMessages"."created_at" IS 'Thời điểm gửi tin nhắn.';

-- ------------------------------------------------------------
-- Seed data (idempotent)
-- ------------------------------------------------------------
INSERT INTO "Roles" ("role_id", "name", "description", "status")
SELECT uuid_generate_v4(), role_name, role_desc, 'active'
FROM (VALUES
    ('admin', 'System administrator'),
    ('user', 'Regular user'),
    ('specialist', 'Skincare specialist')
) AS seed(role_name, role_desc)
WHERE NOT EXISTS (
    SELECT 1 FROM "Roles" r WHERE r."name" = seed.role_name
);

INSERT INTO "Users" ("user_id", "full_name", "email", "google_id", "role_id", "skin_type", "status")
SELECT uuid_generate_v4(), 'Test User', 'test@example.com', 'sample-google-id-1234567890',
       (SELECT "role_id" FROM "Roles" WHERE "name" = 'user' LIMIT 1),
       'combination', 'active'
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "email" = 'test@example.com');

INSERT INTO "Users" ("user_id", "full_name", "email", "google_id", "role_id", "skin_type", "status")
SELECT uuid_generate_v4(), 'Admin User', 'admin@example.com', 'sample-google-id-admin',
       (SELECT "role_id" FROM "Roles" WHERE "name" = 'admin' LIMIT 1),
       NULL, 'active'
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "email" = 'admin@example.com');

INSERT INTO "Symptoms" ("symptom_id", "name", "description")
SELECT uuid_generate_v4(), seed_name, seed_desc
FROM (VALUES
    ('Acne', 'Inflammatory skin condition'),
    ('Dryness', 'Lack of moisture in skin'),
    ('Redness', 'Skin irritation or inflammation')
) AS seed(seed_name, seed_desc)
WHERE NOT EXISTS (
    SELECT 1 FROM "Symptoms" s WHERE s."name" = seed.seed_name
);

INSERT INTO "Questions" ("question_id", "text", "type", "options")
SELECT uuid_generate_v4(), 'What is your skin type?', 'choice', '["oily", "dry", "combination", "sensitive"]'::jsonb
WHERE NOT EXISTS (SELECT 1 FROM "Questions" WHERE "text" = 'What is your skin type?');

INSERT INTO "Questions" ("question_id", "text", "type", "options")
SELECT uuid_generate_v4(), 'Which concerns do you have?', 'multi-choice', '["acne", "wrinkles", "dark spots", "sensitivity"]'::jsonb
WHERE NOT EXISTS (SELECT 1 FROM "Questions" WHERE "text" = 'Which concerns do you have?');

-- ------------------------------------------------------------
-- Validation queries (read-only)
-- ------------------------------------------------------------
-- Verify extensions
SELECT extname, extversion
FROM pg_extension
WHERE extname IN ('uuid-ossp', 'vector');

-- Verify vector columns
SELECT
    c.table_name,
    c.column_name,
    pg_catalog.format_type(a.atttypid, a.atttypmod) AS formatted_type
FROM information_schema.columns c
JOIN pg_class t ON t.relname = c.table_name AND t.relkind = 'r'
JOIN pg_namespace n ON n.oid = t.relnamespace AND n.nspname = c.table_schema
JOIN pg_attribute a ON a.attrelid = t.oid AND a.attname = c.column_name
WHERE c.table_schema = 'public'
  AND c.udt_name = 'vector'
ORDER BY c.table_name, c.column_name;

-- Verify HNSW indexes
SELECT tablename, indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'public'
  AND (indexdef ILIKE '%USING hnsw%' OR indexdef ILIKE '%vector_cosine_ops%');

-- Sample counts
SELECT COUNT(*) AS medicaldocuments_count FROM "MedicalDocuments";
SELECT COUNT(*) AS documentchunks_count FROM "DocumentChunks";

COMMIT;
