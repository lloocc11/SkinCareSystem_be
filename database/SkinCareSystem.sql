
-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS vector;

-- Drop all tables in correct order (reverse of dependencies)
DROP TABLE IF EXISTS ChatMessages CASCADE;
DROP TABLE IF EXISTS ChatSessions CASCADE;
DROP TABLE IF EXISTS ConsentRecords CASCADE;
DROP TABLE IF EXISTS RuleConditions CASCADE;
DROP TABLE IF EXISTS Rules CASCADE;
DROP TABLE IF EXISTS Questions CASCADE;
DROP TABLE IF EXISTS Symptoms CASCADE;
DROP TABLE IF EXISTS RoutineProgress CASCADE;
DROP TABLE IF EXISTS RoutineInstances CASCADE;
DROP TABLE IF EXISTS Feedback CASCADE;
DROP TABLE IF EXISTS RoutineSteps CASCADE;
DROP TABLE IF EXISTS Routines CASCADE;
DROP TABLE IF EXISTS AIResponses CASCADE;
DROP TABLE IF EXISTS QueryMatches CASCADE;
DROP TABLE IF EXISTS UserQueries CASCADE;
DROP TABLE IF EXISTS DocumentChunks CASCADE;
DROP TABLE IF EXISTS MedicalDocuments CASCADE;
DROP TABLE IF EXISTS AIAnalysis CASCADE;
DROP TABLE IF EXISTS UserSymptoms CASCADE;
DROP TABLE IF EXISTS UserAnswers CASCADE;
DROP TABLE IF EXISTS Users CASCADE;
DROP TABLE IF EXISTS Roles CASCADE;

-- Create function to auto-update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ======================
-- CREATE TABLES
-- ======================

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
  "password_hash" varchar NOT NULL,
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

-- Add foreign key after ChatMessages is created
ALTER TABLE "AIAnalysis" ADD FOREIGN KEY ("chat_message_id") REFERENCES "ChatMessages" ("message_id") ON DELETE CASCADE;
ALTER TABLE "UserAnswers" ADD FOREIGN KEY ("question_id") REFERENCES "Questions" ("question_id") ON DELETE CASCADE;
ALTER TABLE "UserSymptoms" ADD FOREIGN KEY ("symptom_id") REFERENCES "Symptoms" ("symptom_id") ON DELETE CASCADE;
ALTER TABLE "Routines" ADD FOREIGN KEY ("analysis_id") REFERENCES "AIAnalysis" ("analysis_id") ON DELETE SET NULL;

-- ======================
-- CREATE INDEXES
-- ======================

-- Roles indexes
CREATE INDEX "idx_roles_role_id" ON "Roles" USING BTREE ("role_id");
CREATE UNIQUE INDEX "idx_roles_name" ON "Roles" USING BTREE ("name");

-- Users indexes
CREATE INDEX "idx_users_user_id" ON "Users" USING BTREE ("user_id");
CREATE UNIQUE INDEX "idx_users_email" ON "Users" USING BTREE ("email");
CREATE INDEX "idx_users_role_id" ON "Users" USING BTREE ("role_id");

-- UserAnswers indexes
CREATE INDEX "idx_useranswers_answer_id" ON "UserAnswers" USING BTREE ("answer_id");
CREATE INDEX "idx_useranswers_user_id" ON "UserAnswers" USING BTREE ("user_id");
CREATE INDEX "idx_useranswers_question_id" ON "UserAnswers" USING BTREE ("question_id");

-- UserSymptoms indexes
CREATE INDEX "idx_usersymptoms_user_symptom_id" ON "UserSymptoms" USING BTREE ("user_symptom_id");
CREATE INDEX "idx_usersymptoms_user_id" ON "UserSymptoms" USING BTREE ("user_id");
CREATE INDEX "idx_usersymptoms_symptom_id" ON "UserSymptoms" USING BTREE ("symptom_id");

-- AIAnalysis indexes
CREATE INDEX "idx_aianalysis_analysis_id" ON "AIAnalysis" USING BTREE ("analysis_id");
CREATE INDEX "idx_aianalysis_user_id" ON "AIAnalysis" USING BTREE ("user_id");
CREATE INDEX "idx_aianalysis_chat_message_id" ON "AIAnalysis" USING BTREE ("chat_message_id");

-- MedicalDocuments indexes
CREATE INDEX "idx_medicaldocuments_doc_id" ON "MedicalDocuments" USING BTREE ("doc_id");
CREATE INDEX "idx_medicaldocuments_created_at" ON "MedicalDocuments" USING BTREE ("created_at");

-- DocumentChunks indexes
CREATE INDEX "idx_documentchunks_chunk_id" ON "DocumentChunks" USING BTREE ("chunk_id");
CREATE INDEX "idx_documentchunks_doc_id" ON "DocumentChunks" USING BTREE ("doc_id");
CREATE INDEX "idx_documentchunks_embedding" ON "DocumentChunks" USING HNSW ("embedding" vector_cosine_ops);
CREATE INDEX "idx_documentchunks_created_at" ON "DocumentChunks" USING BTREE ("created_at");

-- UserQueries indexes
CREATE INDEX "idx_userqueries_query_id" ON "UserQueries" USING BTREE ("query_id");
CREATE INDEX "idx_userqueries_user_id" ON "UserQueries" USING BTREE ("user_id");
CREATE INDEX "idx_userqueries_query_embedding" ON "UserQueries" USING HNSW ("query_embedding" vector_cosine_ops);
CREATE INDEX "idx_userqueries_created_at" ON "UserQueries" USING BTREE ("created_at");

-- QueryMatches indexes
CREATE INDEX "idx_querymatches_match_id" ON "QueryMatches" USING BTREE ("match_id");
CREATE INDEX "idx_querymatches_query_id" ON "QueryMatches" USING BTREE ("query_id");
CREATE INDEX "idx_querymatches_chunk_id" ON "QueryMatches" USING BTREE ("chunk_id");

-- AIResponses indexes
CREATE INDEX "idx_airesponses_response_id" ON "AIResponses" USING BTREE ("response_id");
CREATE INDEX "idx_airesponses_query_id" ON "AIResponses" USING BTREE ("query_id");

-- Routines indexes
CREATE INDEX "idx_routines_routine_id" ON "Routines" USING BTREE ("routine_id");
CREATE INDEX "idx_routines_user_id" ON "Routines" USING BTREE ("user_id");
CREATE INDEX "idx_routines_analysis_id" ON "Routines" USING BTREE ("analysis_id");

-- RoutineSteps indexes
CREATE INDEX "idx_routinesteps_step_id" ON "RoutineSteps" USING BTREE ("step_id");
CREATE INDEX "idx_routinesteps_routine_id" ON "RoutineSteps" USING BTREE ("routine_id");

-- Feedback indexes
CREATE INDEX "idx_feedback_feedback_id" ON "Feedback" USING BTREE ("feedback_id");
CREATE INDEX "idx_feedback_routine_id" ON "Feedback" USING BTREE ("routine_id");
CREATE INDEX "idx_feedback_step_id" ON "Feedback" USING BTREE ("step_id");
CREATE INDEX "idx_feedback_user_id" ON "Feedback" USING BTREE ("user_id");

-- RoutineInstances indexes
CREATE INDEX "idx_routineinstances_instance_id" ON "RoutineInstances" USING BTREE ("instance_id");
CREATE INDEX "idx_routineinstances_routine_id" ON "RoutineInstances" USING BTREE ("routine_id");
CREATE INDEX "idx_routineinstances_user_id" ON "RoutineInstances" USING BTREE ("user_id");

-- RoutineProgress indexes
CREATE INDEX "idx_routineprogress_progress_id" ON "RoutineProgress" USING BTREE ("progress_id");
CREATE INDEX "idx_routineprogress_instance_id" ON "RoutineProgress" USING BTREE ("instance_id");
CREATE INDEX "idx_routineprogress_step_id" ON "RoutineProgress" USING BTREE ("step_id");

-- Symptoms indexes
CREATE INDEX "idx_symptoms_symptom_id" ON "Symptoms" USING BTREE ("symptom_id");
CREATE UNIQUE INDEX "idx_symptoms_name" ON "Symptoms" USING BTREE ("name");

-- Questions indexes
CREATE INDEX "idx_questions_question_id" ON "Questions" USING BTREE ("question_id");

-- Rules indexes
CREATE INDEX "idx_rules_rule_id" ON "Rules" USING BTREE ("rule_id");

-- RuleConditions indexes
CREATE INDEX "idx_ruleconditions_rule_condition_id" ON "RuleConditions" USING BTREE ("rule_condition_id");
CREATE INDEX "idx_ruleconditions_rule_id" ON "RuleConditions" USING BTREE ("rule_id");
CREATE INDEX "idx_ruleconditions_symptom_id" ON "RuleConditions" USING BTREE ("symptom_id");
CREATE INDEX "idx_ruleconditions_question_id" ON "RuleConditions" USING BTREE ("question_id");

-- ConsentRecords indexes
CREATE INDEX "idx_consentrecords_consent_id" ON "ConsentRecords" USING BTREE ("consent_id");
CREATE INDEX "idx_consentrecords_user_id" ON "ConsentRecords" USING BTREE ("user_id");

-- ChatSessions indexes
CREATE INDEX "idx_chatsessions_session_id" ON "ChatSessions" USING BTREE ("session_id");
CREATE INDEX "idx_chatsessions_user_id" ON "ChatSessions" USING BTREE ("user_id");

-- ChatMessages indexes
CREATE INDEX "idx_chatmessages_message_id" ON "ChatMessages" USING BTREE ("message_id");
CREATE INDEX "idx_chatmessages_session_id" ON "ChatMessages" USING BTREE ("session_id");
CREATE INDEX "idx_chatmessages_user_id" ON "ChatMessages" USING BTREE ("user_id");

-- ======================
-- CREATE TRIGGERS
-- ======================

CREATE TRIGGER update_roles_updated_at BEFORE UPDATE ON "Roles" 
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON "Users" 
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_useranswers_updated_at BEFORE UPDATE ON "UserAnswers" 
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_aianalysis_updated_at BEFORE UPDATE ON "AIAnalysis" 
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_routines_updated_at BEFORE UPDATE ON "Routines" 
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_chatsessions_updated_at BEFORE UPDATE ON "ChatSessions" 
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ======================
-- INSERT SAMPLE DATA
-- ======================

-- Insert Roles
INSERT INTO "Roles" (role_id, name, description, status) VALUES
(uuid_generate_v4(), 'admin', 'System administrator', 'active'),
(uuid_generate_v4(), 'user', 'Regular user', 'active'),
(uuid_generate_v4(), 'specialist', 'Skincare specialist', 'active');

-- Insert Sample User
INSERT INTO "Users" (user_id, full_name, email, password_hash, role_id, skin_type, status) VALUES
(uuid_generate_v4(), 'Test User', 'test@example.com', '$2a$10$abcdefghijklmnopqrstuv', 
 (SELECT role_id FROM "Roles" WHERE name = 'user'), 'combination', 'active');

-- Insert Sample Symptoms
INSERT INTO "Symptoms" (symptom_id, name, description) VALUES
(uuid_generate_v4(), 'Acne', 'Inflammatory skin condition'),
(uuid_generate_v4(), 'Dryness', 'Lack of moisture in skin'),
(uuid_generate_v4(), 'Redness', 'Skin irritation or inflammation');

-- Insert Sample Questions
INSERT INTO "Questions" (question_id, text, type, options) VALUES
(uuid_generate_v4(), 'What is your skin type?', 'choice', 
 '["oily", "dry", "combination", "sensitive"]'::jsonb),
(uuid_generate_v4(), 'Which concerns do you have?', 'multi-choice', 
 '["acne", "wrinkles", "dark spots", "sensitivity"]'::jsonb);

COMMENT ON DATABASE CURRENT_DATABASE IS 'Skincare application database - Setup complete';