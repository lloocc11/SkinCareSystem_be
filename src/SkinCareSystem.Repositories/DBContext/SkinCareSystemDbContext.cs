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

    public virtual DbSet<AIAnalysis> AIAnalyses { get; set; }

    public virtual DbSet<AIResponse> AIResponses { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatSession> ChatSessions { get; set; }

    public virtual DbSet<ConsentRecord> ConsentRecords { get; set; }

    public virtual DbSet<DocumentChunk> DocumentChunks { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<MedicalDocument> MedicalDocuments { get; set; }

    public virtual DbSet<MedicalDocumentAsset> MedicalDocumentAssets { get; set; }

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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=ep-icy-feather-a1zdymy3-pooler.ap-southeast-1.aws.neon.tech;Database=skincaresystem_db;Username=neondb_owner;Password=npg_Qpl4BdHAOY5V;SSL Mode=VerifyFull;Channel Binding=Require;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("uuid-ossp")
            .HasPostgresExtension("vector");

        modelBuilder.Entity<AIAnalysis>(entity =>
        {
            entity.HasKey(e => e.analysis_id).HasName("AIAnalysis_pkey");

            entity.ToTable("AIAnalysis");

            entity.HasIndex(e => e.analysis_id, "idx_aianalysis_analysis_id");

            entity.HasIndex(e => e.chat_message_id, "idx_aianalysis_chat_message_id");

            entity.HasIndex(e => e.user_id, "idx_aianalysis_user_id");

            entity.Property(e => e.analysis_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.result).HasColumnType("jsonb");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.chat_message).WithMany(p => p.AIAnalyses)
                .HasForeignKey(d => d.chat_message_id)
                .HasConstraintName("AIAnalysis_chat_message_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.AIAnalyses)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("AIAnalysis_user_id_fkey");
        });

        modelBuilder.Entity<AIResponse>(entity =>
        {
            entity.HasKey(e => e.response_id).HasName("AIResponses_pkey");

            entity.HasIndex(e => e.query_id, "idx_airesponses_query_id");

            entity.HasIndex(e => e.response_id, "idx_airesponses_response_id");

            entity.Property(e => e.response_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.response_type).HasColumnType("character varying");

            entity.HasOne(d => d.query).WithMany(p => p.AIResponses)
                .HasForeignKey(d => d.query_id)
                .HasConstraintName("AIResponses_query_id_fkey");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.message_id).HasName("ChatMessages_pkey");

            entity.HasIndex(e => e.message_id, "idx_chatmessages_message_id");

            entity.HasIndex(e => e.session_id, "idx_chatmessages_session_id");

            entity.HasIndex(e => e.user_id, "idx_chatmessages_user_id");

            entity.Property(e => e.message_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.image_url).HasColumnType("character varying");
            entity.Property(e => e.message_type).HasColumnType("character varying");
            entity.Property(e => e.role).HasColumnType("character varying");

            entity.HasOne(d => d.session).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.session_id)
                .HasConstraintName("ChatMessages_session_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ChatMessages_user_id_fkey");
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.session_id).HasName("ChatSessions_pkey");

            entity.HasIndex(e => e.session_id, "idx_chatsessions_session_id");

            entity.HasIndex(e => e.user_id, "idx_chatsessions_user_id");

            entity.Property(e => e.session_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.title).HasColumnType("character varying");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.user).WithMany(p => p.ChatSessions)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("ChatSessions_user_id_fkey");
        });

        modelBuilder.Entity<ConsentRecord>(entity =>
        {
            entity.HasKey(e => e.consent_id).HasName("ConsentRecords_pkey");

            entity.HasIndex(e => e.consent_id, "idx_consentrecords_consent_id");

            entity.HasIndex(e => e.user_id, "idx_consentrecords_user_id");

            entity.Property(e => e.consent_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.consent_type).HasColumnType("character varying");
            entity.Property(e => e.given_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.user).WithMany(p => p.ConsentRecords)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("ConsentRecords_user_id_fkey");
        });

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.HasKey(e => e.chunk_id).HasName("DocumentChunks_pkey");

            entity.HasIndex(e => e.chunk_id, "idx_documentchunks_chunk_id");

            entity.HasIndex(e => e.created_at, "idx_documentchunks_created_at");

            entity.HasIndex(e => e.doc_id, "idx_documentchunks_doc_id");

            entity.Property(e => e.chunk_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.doc).WithMany(p => p.DocumentChunks)
                .HasForeignKey(d => d.doc_id)
                .HasConstraintName("DocumentChunks_doc_id_fkey");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.feedback_id).HasName("Feedback_pkey");

            entity.ToTable("Feedback");

            entity.HasIndex(e => e.feedback_id, "idx_feedback_feedback_id");

            entity.HasIndex(e => e.routine_id, "idx_feedback_routine_id");

            entity.HasIndex(e => e.step_id, "idx_feedback_step_id");

            entity.HasIndex(e => e.user_id, "idx_feedback_user_id");

            entity.Property(e => e.feedback_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.routine).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.routine_id)
                .HasConstraintName("Feedback_routine_id_fkey");

            entity.HasOne(d => d.step).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.step_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Feedback_step_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("Feedback_user_id_fkey");
        });

        modelBuilder.Entity<MedicalDocument>(entity =>
        {
            entity.HasKey(e => e.doc_id).HasName("MedicalDocuments_pkey");

            entity.HasIndex(e => e.created_at, "idx_medicaldocuments_created_at");

            entity.HasIndex(e => e.doc_id, "idx_medicaldocuments_doc_id");

            entity.Property(e => e.doc_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.last_updated).HasColumnType("timestamp without time zone");
            entity.Property(e => e.source).HasColumnType("character varying");
            entity.Property(e => e.status).HasColumnType("character varying");
            entity.Property(e => e.title).HasColumnType("character varying");
        });

        modelBuilder.Entity<MedicalDocumentAsset>(entity =>
        {
            entity.HasKey(e => e.asset_id).HasName("MedicalDocumentAssets_pkey");

            entity.HasIndex(e => e.doc_id, "idx_medicaldocumentassets_doc_id");

            entity.HasIndex(e => e.public_id, "idx_medicaldocumentassets_public_id");

            entity.HasIndex(e => new { e.doc_id, e.file_url }, "uq_medicaldocumentassets_doc_url").IsUnique();

            entity.Property(e => e.asset_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.mime_type).HasColumnType("character varying");
            entity.Property(e => e.provider)
                .HasDefaultValueSql("'cloudinary'::character varying")
                .HasColumnType("character varying");
            entity.Property(e => e.public_id).HasColumnType("character varying");

            entity.HasOne(d => d.doc).WithMany(p => p.MedicalDocumentAssets)
                .HasForeignKey(d => d.doc_id)
                .HasConstraintName("fk_medicaldocumentassets_doc");
        });

        modelBuilder.Entity<QueryMatch>(entity =>
        {
            entity.HasKey(e => e.match_id).HasName("QueryMatches_pkey");

            entity.HasIndex(e => e.chunk_id, "idx_querymatches_chunk_id");

            entity.HasIndex(e => e.match_id, "idx_querymatches_match_id");

            entity.HasIndex(e => e.query_id, "idx_querymatches_query_id");

            entity.Property(e => e.match_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.chunk).WithMany(p => p.QueryMatches)
                .HasForeignKey(d => d.chunk_id)
                .HasConstraintName("QueryMatches_chunk_id_fkey");

            entity.HasOne(d => d.query).WithMany(p => p.QueryMatches)
                .HasForeignKey(d => d.query_id)
                .HasConstraintName("QueryMatches_query_id_fkey");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.question_id).HasName("Questions_pkey");

            entity.HasIndex(e => e.question_id, "idx_questions_question_id");

            entity.Property(e => e.question_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.options).HasColumnType("jsonb");
            entity.Property(e => e.text).HasColumnType("character varying");
            entity.Property(e => e.type).HasColumnType("character varying");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.role_id).HasName("Roles_pkey");

            entity.HasIndex(e => e.name, "Roles_name_key").IsUnique();

            entity.HasIndex(e => e.name, "idx_roles_name").IsUnique();

            entity.HasIndex(e => e.role_id, "idx_roles_role_id");

            entity.Property(e => e.role_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.name).HasColumnType("character varying");
            entity.Property(e => e.status).HasColumnType("character varying");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<Routine>(entity =>
        {
            entity.HasKey(e => e.routine_id).HasName("Routines_pkey");

            entity.HasIndex(e => e.analysis_id, "idx_routines_analysis_id");

            entity.HasIndex(e => e.routine_id, "idx_routines_routine_id");

            entity.HasIndex(e => e.user_id, "idx_routines_user_id");

            entity.Property(e => e.routine_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.status).HasColumnType("character varying");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.version).HasDefaultValue(1);

            entity.HasOne(d => d.analysis).WithMany(p => p.Routines)
                .HasForeignKey(d => d.analysis_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Routines_analysis_id_fkey");

            entity.HasOne(d => d.parent_routine).WithMany(p => p.Inverseparent_routine)
                .HasForeignKey(d => d.parent_routine_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Routines_parent_routine_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.Routines)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("Routines_user_id_fkey");
        });

        modelBuilder.Entity<RoutineInstance>(entity =>
        {
            entity.HasKey(e => e.instance_id).HasName("RoutineInstances_pkey");

            entity.HasIndex(e => e.instance_id, "idx_routineinstances_instance_id");

            entity.HasIndex(e => e.routine_id, "idx_routineinstances_routine_id");

            entity.HasIndex(e => e.user_id, "idx_routineinstances_user_id");

            entity.Property(e => e.instance_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.status).HasColumnType("character varying");

            entity.HasOne(d => d.routine).WithMany(p => p.RoutineInstances)
                .HasForeignKey(d => d.routine_id)
                .HasConstraintName("RoutineInstances_routine_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.RoutineInstances)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("RoutineInstances_user_id_fkey");
        });

        modelBuilder.Entity<RoutineProgress>(entity =>
        {
            entity.HasKey(e => e.progress_id).HasName("RoutineProgress_pkey");

            entity.ToTable("RoutineProgress");

            entity.HasIndex(e => e.instance_id, "idx_routineprogress_instance_id");

            entity.HasIndex(e => e.progress_id, "idx_routineprogress_progress_id");

            entity.HasIndex(e => e.step_id, "idx_routineprogress_step_id");

            entity.Property(e => e.progress_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.completed_at).HasColumnType("timestamp without time zone");
            entity.Property(e => e.photo_url).HasColumnType("character varying");
            entity.Property(e => e.status)
                .HasDefaultValueSql("'completed'::character varying")
                .HasColumnType("character varying");

            entity.HasOne(d => d.instance).WithMany(p => p.RoutineProgresses)
                .HasForeignKey(d => d.instance_id)
                .HasConstraintName("RoutineProgress_instance_id_fkey");

            entity.HasOne(d => d.step).WithMany(p => p.RoutineProgresses)
                .HasForeignKey(d => d.step_id)
                .HasConstraintName("RoutineProgress_step_id_fkey");
        });

        modelBuilder.Entity<RoutineStep>(entity =>
        {
            entity.HasKey(e => e.step_id).HasName("RoutineSteps_pkey");

            entity.HasIndex(e => e.routine_id, "idx_routinesteps_routine_id");

            entity.HasIndex(e => e.step_id, "idx_routinesteps_step_id");

            entity.Property(e => e.step_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.frequency)
                .HasDefaultValueSql("'daily'::character varying")
                .HasColumnType("character varying");
            entity.Property(e => e.time_of_day).HasColumnType("character varying");

            entity.HasOne(d => d.routine).WithMany(p => p.RoutineSteps)
                .HasForeignKey(d => d.routine_id)
                .HasConstraintName("RoutineSteps_routine_id_fkey");
        });

        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(e => e.rule_id).HasName("Rules_pkey");

            entity.HasIndex(e => e.rule_id, "idx_rules_rule_id");

            entity.Property(e => e.rule_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.urgency_level).HasColumnType("character varying");
        });

        modelBuilder.Entity<RuleCondition>(entity =>
        {
            entity.HasKey(e => e.rule_condition_id).HasName("RuleConditions_pkey");

            entity.HasIndex(e => e.question_id, "idx_ruleconditions_question_id");

            entity.HasIndex(e => e.rule_condition_id, "idx_ruleconditions_rule_condition_id");

            entity.HasIndex(e => e.rule_id, "idx_ruleconditions_rule_id");

            entity.HasIndex(e => e.symptom_id, "idx_ruleconditions_symptom_id");

            entity.Property(e => e.rule_condition_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e._operator)
                .HasColumnType("character varying")
                .HasColumnName("operator");

            entity.HasOne(d => d.question).WithMany(p => p.RuleConditions)
                .HasForeignKey(d => d.question_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("RuleConditions_question_id_fkey");

            entity.HasOne(d => d.rule).WithMany(p => p.RuleConditions)
                .HasForeignKey(d => d.rule_id)
                .HasConstraintName("RuleConditions_rule_id_fkey");

            entity.HasOne(d => d.symptom).WithMany(p => p.RuleConditions)
                .HasForeignKey(d => d.symptom_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("RuleConditions_symptom_id_fkey");
        });

        modelBuilder.Entity<Symptom>(entity =>
        {
            entity.HasKey(e => e.symptom_id).HasName("Symptoms_pkey");

            entity.HasIndex(e => e.name, "Symptoms_name_key").IsUnique();

            entity.HasIndex(e => e.name, "idx_symptoms_name").IsUnique();

            entity.HasIndex(e => e.symptom_id, "idx_symptoms_symptom_id");

            entity.Property(e => e.symptom_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.example_image_url).HasColumnType("character varying");
            entity.Property(e => e.name).HasColumnType("character varying");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("Users_pkey");

            entity.HasIndex(e => e.email, "Users_email_key").IsUnique();

            entity.HasIndex(e => e.google_id, "Users_google_id_key").IsUnique();

            entity.HasIndex(e => e.email, "idx_users_email").IsUnique();

            entity.HasIndex(e => e.role_id, "idx_users_role_id");

            entity.HasIndex(e => e.user_id, "idx_users_user_id");

            entity.Property(e => e.user_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.email).HasColumnType("character varying");
            entity.Property(e => e.full_name).HasColumnType("character varying");
            entity.Property(e => e.google_id).HasColumnType("character varying");
            entity.Property(e => e.skin_type).HasColumnType("character varying");
            entity.Property(e => e.status).HasColumnType("character varying");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.role).WithMany(p => p.Users)
                .HasForeignKey(d => d.role_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Users_role_id_fkey");
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.answer_id).HasName("UserAnswers_pkey");

            entity.HasIndex(e => e.answer_id, "idx_useranswers_answer_id");

            entity.HasIndex(e => e.question_id, "idx_useranswers_question_id");

            entity.HasIndex(e => e.user_id, "idx_useranswers_user_id");

            entity.HasIndex(e => new { e.user_id, e.question_id }, "uq_useranswers").IsUnique();

            entity.Property(e => e.answer_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.question).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.question_id)
                .HasConstraintName("UserAnswers_question_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("UserAnswers_user_id_fkey");
        });

        modelBuilder.Entity<UserQuery>(entity =>
        {
            entity.HasKey(e => e.query_id).HasName("UserQueries_pkey");

            entity.HasIndex(e => e.created_at, "idx_userqueries_created_at");

            entity.HasIndex(e => e.query_id, "idx_userqueries_query_id");

            entity.HasIndex(e => e.user_id, "idx_userqueries_user_id");

            entity.Property(e => e.query_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.user).WithMany(p => p.UserQueries)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("UserQueries_user_id_fkey");
        });

        modelBuilder.Entity<UserSymptom>(entity =>
        {
            entity.HasKey(e => e.user_symptom_id).HasName("UserSymptoms_pkey");

            entity.HasIndex(e => e.symptom_id, "idx_usersymptoms_symptom_id");

            entity.HasIndex(e => e.user_id, "idx_usersymptoms_user_id");

            entity.HasIndex(e => e.user_symptom_id, "idx_usersymptoms_user_symptom_id");

            entity.HasIndex(e => new { e.user_id, e.symptom_id }, "uq_usersymptoms").IsUnique();

            entity.Property(e => e.user_symptom_id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.reported_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.symptom).WithMany(p => p.UserSymptoms)
                .HasForeignKey(d => d.symptom_id)
                .HasConstraintName("UserSymptoms_symptom_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.UserSymptoms)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("UserSymptoms_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
