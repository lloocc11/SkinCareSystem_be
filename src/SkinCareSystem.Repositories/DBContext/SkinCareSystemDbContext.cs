using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.DBContext;

public partial class SkinCareSystemDbContext : DbContext
{
    public SkinCareSystemDbContext()
    {
    }

    public SkinCareSystemDbContext(DbContextOptions<SkinCareSystemDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aianalysis> Aianalyses { get; set; }

    public virtual DbSet<Airesponse> Airesponses { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatSession> ChatSessions { get; set; }

    public virtual DbSet<ConsentRecord> ConsentRecords { get; set; }

    public virtual DbSet<DocumentChunk> DocumentChunks { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<MedicalDocument> MedicalDocuments { get; set; }

    public virtual DbSet<QueryMatch> QueryMatches { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Routine> Routines { get; set; }

    public virtual DbSet<RoutineInstance> RoutineInstances { get; set; }

    public virtual DbSet<RoutineProgress> RoutineProgresses { get; set; }

    public virtual DbSet<RoutineStep> RoutineSteps { get; set; }

    public virtual DbSet<Rule> Rules { get; set; }

    public virtual DbSet<RuleCondition> RuleConditions { get; set; }

    public virtual DbSet<Symptom> Symptoms { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    public virtual DbSet<UserQuery> UserQueries { get; set; }

    public virtual DbSet<UserSymptom> UserSymptoms { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("uuid-ossp")
            .HasPostgresExtension("vector");

        modelBuilder.Entity<Aianalysis>(entity =>
        {
            entity.HasKey(e => e.AnalysisId).HasName("AIAnalysis_pkey");

            entity.ToTable("AIAnalysis");

            entity.HasIndex(e => e.AnalysisId, "idx_aianalysis_analysis_id");

            entity.HasIndex(e => e.ChatMessageId, "idx_aianalysis_chat_message_id");

            entity.HasIndex(e => e.UserId, "idx_aianalysis_user_id");

            entity.Property(e => e.AnalysisId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("analysis_id");
            entity.Property(e => e.ChatMessageId).HasColumnName("chat_message_id");
            entity.Property(e => e.Confidence).HasColumnName("confidence");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.RawInput).HasColumnName("raw_input");
            entity.Property(e => e.Result)
                .HasColumnType("jsonb")
                .HasColumnName("result");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ChatMessage).WithMany(p => p.Aianalyses)
                .HasForeignKey(d => d.ChatMessageId)
                .HasConstraintName("AIAnalysis_chat_message_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Aianalyses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("AIAnalysis_user_id_fkey");
        });

        modelBuilder.Entity<Airesponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("AIResponses_pkey");

            entity.ToTable("AIResponses");

            entity.HasIndex(e => e.QueryId, "idx_airesponses_query_id");

            entity.HasIndex(e => e.ResponseId, "idx_airesponses_response_id");

            entity.Property(e => e.ResponseId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("response_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.QueryId).HasColumnName("query_id");
            entity.Property(e => e.ResponseText).HasColumnName("response_text");
            entity.Property(e => e.ResponseType)
                .HasColumnType("character varying")
                .HasColumnName("response_type");

            entity.HasOne(d => d.Query).WithMany(p => p.Airesponses)
                .HasForeignKey(d => d.QueryId)
                .HasConstraintName("AIResponses_query_id_fkey");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("ChatMessages_pkey");

            entity.HasIndex(e => e.MessageId, "idx_chatmessages_message_id");

            entity.HasIndex(e => e.SessionId, "idx_chatmessages_session_id");

            entity.HasIndex(e => e.UserId, "idx_chatmessages_user_id");

            entity.Property(e => e.MessageId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("message_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl)
                .HasColumnType("character varying")
                .HasColumnName("image_url");
            entity.Property(e => e.MessageType)
                .HasColumnType("character varying")
                .HasColumnName("message_type");
            entity.Property(e => e.Role)
                .HasColumnType("character varying")
                .HasColumnName("role");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Session).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("ChatMessages_session_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("ChatMessages_user_id_fkey");
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("ChatSessions_pkey");

            entity.HasIndex(e => e.SessionId, "idx_chatsessions_session_id");

            entity.HasIndex(e => e.UserId, "idx_chatsessions_user_id");

            entity.Property(e => e.SessionId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("session_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Title)
                .HasColumnType("character varying")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.ChatSessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("ChatSessions_user_id_fkey");
        });

        modelBuilder.Entity<ConsentRecord>(entity =>
        {
            entity.HasKey(e => e.ConsentId).HasName("ConsentRecords_pkey");

            entity.HasIndex(e => e.ConsentId, "idx_consentrecords_consent_id");

            entity.HasIndex(e => e.UserId, "idx_consentrecords_user_id");

            entity.Property(e => e.ConsentId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("consent_id");
            entity.Property(e => e.ConsentText).HasColumnName("consent_text");
            entity.Property(e => e.ConsentType)
                .HasColumnType("character varying")
                .HasColumnName("consent_type");
            entity.Property(e => e.Given).HasColumnName("given");
            entity.Property(e => e.GivenAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("given_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.ConsentRecords)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("ConsentRecords_user_id_fkey");
        });

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.HasKey(e => e.ChunkId).HasName("DocumentChunks_pkey");

            entity.HasIndex(e => e.ChunkId, "idx_documentchunks_chunk_id");

            entity.HasIndex(e => e.CreatedAt, "idx_documentchunks_created_at");

            entity.HasIndex(e => e.DocId, "idx_documentchunks_doc_id");

            entity.Property(e => e.ChunkId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("chunk_id");
            entity.Property(e => e.ChunkText).HasColumnName("chunk_text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DocId).HasColumnName("doc_id");

            entity.HasOne(d => d.Doc).WithMany(p => p.DocumentChunks)
                .HasForeignKey(d => d.DocId)
                .HasConstraintName("DocumentChunks_doc_id_fkey");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("Feedback_pkey");

            entity.ToTable("Feedback");

            entity.HasIndex(e => e.FeedbackId, "idx_feedback_feedback_id");

            entity.HasIndex(e => e.RoutineId, "idx_feedback_routine_id");

            entity.HasIndex(e => e.StepId, "idx_feedback_step_id");

            entity.HasIndex(e => e.UserId, "idx_feedback_user_id");

            entity.Property(e => e.FeedbackId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("feedback_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.RoutineId).HasColumnName("routine_id");
            entity.Property(e => e.StepId).HasColumnName("step_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Routine).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.RoutineId)
                .HasConstraintName("Feedback_routine_id_fkey");

            entity.HasOne(d => d.Step).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.StepId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Feedback_step_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Feedback_user_id_fkey");
        });

        modelBuilder.Entity<MedicalDocument>(entity =>
        {
            entity.HasKey(e => e.DocId).HasName("MedicalDocuments_pkey");

            entity.HasIndex(e => e.CreatedAt, "idx_medicaldocuments_created_at");

            entity.HasIndex(e => e.DocId, "idx_medicaldocuments_doc_id");

            entity.Property(e => e.DocId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("doc_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.LastUpdated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_updated");
            entity.Property(e => e.Source)
                .HasColumnType("character varying")
                .HasColumnName("source");
            entity.Property(e => e.Status)
                .HasColumnType("character varying")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasColumnType("character varying")
                .HasColumnName("title");
        });

        modelBuilder.Entity<QueryMatch>(entity =>
        {
            entity.HasKey(e => e.MatchId).HasName("QueryMatches_pkey");

            entity.HasIndex(e => e.ChunkId, "idx_querymatches_chunk_id");

            entity.HasIndex(e => e.MatchId, "idx_querymatches_match_id");

            entity.HasIndex(e => e.QueryId, "idx_querymatches_query_id");

            entity.Property(e => e.MatchId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("match_id");
            entity.Property(e => e.ChunkId).HasColumnName("chunk_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.QueryId).HasColumnName("query_id");
            entity.Property(e => e.SimilarityScore).HasColumnName("similarity_score");

            entity.HasOne(d => d.Chunk).WithMany(p => p.QueryMatches)
                .HasForeignKey(d => d.ChunkId)
                .HasConstraintName("QueryMatches_chunk_id_fkey");

            entity.HasOne(d => d.Query).WithMany(p => p.QueryMatches)
                .HasForeignKey(d => d.QueryId)
                .HasConstraintName("QueryMatches_query_id_fkey");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("Questions_pkey");

            entity.HasIndex(e => e.QuestionId, "idx_questions_question_id");

            entity.Property(e => e.QuestionId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("question_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Options)
                .HasColumnType("jsonb")
                .HasColumnName("options");
            entity.Property(e => e.Text)
                .HasColumnType("character varying")
                .HasColumnName("text");
            entity.Property(e => e.Type)
                .HasColumnType("character varying")
                .HasColumnName("type");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("Roles_pkey");

            entity.HasIndex(e => e.Name, "Roles_name_key").IsUnique();

            entity.HasIndex(e => e.Name, "idx_roles_name").IsUnique();

            entity.HasIndex(e => e.RoleId, "idx_roles_role_id");

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("role_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasColumnType("character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Routine>(entity =>
        {
            entity.HasKey(e => e.RoutineId).HasName("Routines_pkey");

            entity.HasIndex(e => e.AnalysisId, "idx_routines_analysis_id");

            entity.HasIndex(e => e.RoutineId, "idx_routines_routine_id");

            entity.HasIndex(e => e.UserId, "idx_routines_user_id");

            entity.Property(e => e.RoutineId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("routine_id");
            entity.Property(e => e.AnalysisId).HasColumnName("analysis_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ParentRoutineId).HasColumnName("parent_routine_id");
            entity.Property(e => e.Status)
                .HasColumnType("character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Version)
                .HasDefaultValue(1)
                .HasColumnName("version");

            entity.HasOne(d => d.Analysis).WithMany(p => p.Routines)
                .HasForeignKey(d => d.AnalysisId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Routines_analysis_id_fkey");

            entity.HasOne(d => d.ParentRoutine).WithMany(p => p.InverseParentRoutine)
                .HasForeignKey(d => d.ParentRoutineId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Routines_parent_routine_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Routines)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Routines_user_id_fkey");
        });

        modelBuilder.Entity<RoutineInstance>(entity =>
        {
            entity.HasKey(e => e.InstanceId).HasName("RoutineInstances_pkey");

            entity.HasIndex(e => e.InstanceId, "idx_routineinstances_instance_id");

            entity.HasIndex(e => e.RoutineId, "idx_routineinstances_routine_id");

            entity.HasIndex(e => e.UserId, "idx_routineinstances_user_id");

            entity.Property(e => e.InstanceId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("instance_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.RoutineId).HasColumnName("routine_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasColumnType("character varying")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Routine).WithMany(p => p.RoutineInstances)
                .HasForeignKey(d => d.RoutineId)
                .HasConstraintName("RoutineInstances_routine_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.RoutineInstances)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("RoutineInstances_user_id_fkey");
        });

        modelBuilder.Entity<RoutineProgress>(entity =>
        {
            entity.HasKey(e => e.ProgressId).HasName("RoutineProgress_pkey");

            entity.ToTable("RoutineProgress");

            entity.HasIndex(e => e.InstanceId, "idx_routineprogress_instance_id");

            entity.HasIndex(e => e.ProgressId, "idx_routineprogress_progress_id");

            entity.HasIndex(e => e.StepId, "idx_routineprogress_step_id");

            entity.Property(e => e.ProgressId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("progress_id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.InstanceId).HasColumnName("instance_id");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.PhotoUrl)
                .HasColumnType("character varying")
                .HasColumnName("photo_url");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'completed'::character varying")
                .HasColumnType("character varying")
                .HasColumnName("status");
            entity.Property(e => e.StepId).HasColumnName("step_id");

            entity.HasOne(d => d.Instance).WithMany(p => p.RoutineProgresses)
                .HasForeignKey(d => d.InstanceId)
                .HasConstraintName("RoutineProgress_instance_id_fkey");

            entity.HasOne(d => d.Step).WithMany(p => p.RoutineProgresses)
                .HasForeignKey(d => d.StepId)
                .HasConstraintName("RoutineProgress_step_id_fkey");
        });

        modelBuilder.Entity<RoutineStep>(entity =>
        {
            entity.HasKey(e => e.StepId).HasName("RoutineSteps_pkey");

            entity.HasIndex(e => e.RoutineId, "idx_routinesteps_routine_id");

            entity.HasIndex(e => e.StepId, "idx_routinesteps_step_id");

            entity.Property(e => e.StepId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("step_id");
            entity.Property(e => e.Frequency)
                .HasDefaultValueSql("'daily'::character varying")
                .HasColumnType("character varying")
                .HasColumnName("frequency");
            entity.Property(e => e.Instruction).HasColumnName("instruction");
            entity.Property(e => e.RoutineId).HasColumnName("routine_id");
            entity.Property(e => e.StepOrder).HasColumnName("step_order");
            entity.Property(e => e.TimeOfDay)
                .HasColumnType("character varying")
                .HasColumnName("time_of_day");

            entity.HasOne(d => d.Routine).WithMany(p => p.RoutineSteps)
                .HasForeignKey(d => d.RoutineId)
                .HasConstraintName("RoutineSteps_routine_id_fkey");
        });

        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("Rules_pkey");

            entity.HasIndex(e => e.RuleId, "idx_rules_rule_id");

            entity.Property(e => e.RuleId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("rule_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Recommendation).HasColumnName("recommendation");
            entity.Property(e => e.UrgencyLevel)
                .HasColumnType("character varying")
                .HasColumnName("urgency_level");
        });

        modelBuilder.Entity<RuleCondition>(entity =>
        {
            entity.HasKey(e => e.RuleConditionId).HasName("RuleConditions_pkey");

            entity.HasIndex(e => e.QuestionId, "idx_ruleconditions_question_id");

            entity.HasIndex(e => e.RuleConditionId, "idx_ruleconditions_rule_condition_id");

            entity.HasIndex(e => e.RuleId, "idx_ruleconditions_rule_id");

            entity.HasIndex(e => e.SymptomId, "idx_ruleconditions_symptom_id");

            entity.Property(e => e.RuleConditionId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("rule_condition_id");
            entity.Property(e => e.Operator)
                .HasColumnType("character varying")
                .HasColumnName("operator");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.RuleId).HasColumnName("rule_id");
            entity.Property(e => e.SymptomId).HasColumnName("symptom_id");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Question).WithMany(p => p.RuleConditions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("RuleConditions_question_id_fkey");

            entity.HasOne(d => d.Rule).WithMany(p => p.RuleConditions)
                .HasForeignKey(d => d.RuleId)
                .HasConstraintName("RuleConditions_rule_id_fkey");

            entity.HasOne(d => d.Symptom).WithMany(p => p.RuleConditions)
                .HasForeignKey(d => d.SymptomId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("RuleConditions_symptom_id_fkey");
        });

        modelBuilder.Entity<Symptom>(entity =>
        {
            entity.HasKey(e => e.SymptomId).HasName("Symptoms_pkey");

            entity.HasIndex(e => e.Name, "Symptoms_name_key").IsUnique();

            entity.HasIndex(e => e.Name, "idx_symptoms_name").IsUnique();

            entity.HasIndex(e => e.SymptomId, "idx_symptoms_symptom_id");

            entity.Property(e => e.SymptomId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("symptom_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ExampleImageUrl)
                .HasColumnType("character varying")
                .HasColumnName("example_image_url");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "Users_email_key").IsUnique();

            entity.HasIndex(e => e.GoogleId, "Users_google_id_key").IsUnique();

            entity.HasIndex(e => e.Email, "idx_users_email").IsUnique();

            entity.HasIndex(e => e.RoleId, "idx_users_role_id");

            entity.HasIndex(e => e.UserId, "idx_users_user_id");

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasColumnType("character varying")
                .HasColumnName("full_name");
            entity.Property(e => e.GoogleId)
                .HasColumnType("character varying")
                .HasColumnName("google_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.SkinType)
                .HasColumnType("character varying")
                .HasColumnName("skin_type");
            entity.Property(e => e.Status)
                .HasColumnType("character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Users_role_id_fkey");
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("UserAnswers_pkey");

            entity.HasIndex(e => e.AnswerId, "idx_useranswers_answer_id");

            entity.HasIndex(e => e.QuestionId, "idx_useranswers_question_id");

            entity.HasIndex(e => e.UserId, "idx_useranswers_user_id");

            entity.Property(e => e.AnswerId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("answer_id");
            entity.Property(e => e.AnswerValue).HasColumnName("answer_value");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("UserAnswers_question_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("UserAnswers_user_id_fkey");
        });

        modelBuilder.Entity<UserQuery>(entity =>
        {
            entity.HasKey(e => e.QueryId).HasName("UserQueries_pkey");

            entity.HasIndex(e => e.CreatedAt, "idx_userqueries_created_at");

            entity.HasIndex(e => e.QueryId, "idx_userqueries_query_id");

            entity.HasIndex(e => e.UserId, "idx_userqueries_user_id");

            entity.Property(e => e.QueryId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("query_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.QueryText).HasColumnName("query_text");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserQueries)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("UserQueries_user_id_fkey");
        });

        modelBuilder.Entity<UserSymptom>(entity =>
        {
            entity.HasKey(e => e.UserSymptomId).HasName("UserSymptoms_pkey");

            entity.HasIndex(e => e.SymptomId, "idx_usersymptoms_symptom_id");

            entity.HasIndex(e => e.UserId, "idx_usersymptoms_user_id");

            entity.HasIndex(e => e.UserSymptomId, "idx_usersymptoms_user_symptom_id");

            entity.Property(e => e.UserSymptomId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("user_symptom_id");
            entity.Property(e => e.ReportedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reported_at");
            entity.Property(e => e.SymptomId).HasColumnName("symptom_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Symptom).WithMany(p => p.UserSymptoms)
                .HasForeignKey(d => d.SymptomId)
                .HasConstraintName("UserSymptoms_symptom_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserSymptoms)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("UserSymptoms_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
