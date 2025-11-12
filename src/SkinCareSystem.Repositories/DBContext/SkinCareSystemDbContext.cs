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
        => optionsBuilder.UseNpgsql("Host=ep-icy-feather-a1zdymy3-pooler.ap-southeast-1.aws.neon.tech;Database=skincaresystem_db;Username=neondb_owner;Password=npg_Qpl4BdHAOY5V;SSL Mode=VerifyFull;Channel Binding=Require;", x => x.UseVector());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("uuid-ossp")
            .HasPostgresExtension("vector");

        modelBuilder.Entity<AIAnalysis>(entity =>
        {
            entity.HasKey(e => e.analysis_id).HasName("AIAnalysis_pkey");

            entity.ToTable("AIAnalysis", tb => tb.HasComment("Kết quả phân tích AI cho từng tin nhắn."));

            entity.HasIndex(e => e.analysis_id, "idx_aianalysis_analysis_id");

            entity.HasIndex(e => e.chat_message_id, "idx_aianalysis_chat_message_id");

            entity.HasIndex(e => e.user_id, "idx_aianalysis_user_id");

            entity.Property(e => e.analysis_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính bản ghi phân tích.");
            entity.Property(e => e.chat_message_id).HasComment("Tin nhắn nguồn được phân tích.");
            entity.Property(e => e.confidence).HasComment("Độ tin cậy của phân tích (0-1).");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo phân tích.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.raw_input).HasComment("Dữ liệu đầu vào gốc.");
            entity.Property(e => e.result)
                .HasComment("Kết quả phân tích dạng JSON.")
                .HasColumnType("jsonb");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm cập nhật phân tích.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.user_id).HasComment("Người dùng được phân tích.");

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

            entity.ToTable(tb => tb.HasComment("Phản hồi AI sau truy vấn người dùng."));

            entity.HasIndex(e => e.query_id, "idx_airesponses_query_id");

            entity.HasIndex(e => e.response_id, "idx_airesponses_response_id");

            entity.Property(e => e.response_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính phản hồi.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo phản hồi.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.query_id).HasComment("Truy vấn nguồn.");
            entity.Property(e => e.response_text).HasComment("Nội dung phản hồi.");
            entity.Property(e => e.response_type)
                .HasComment("Loại phản hồi (recommendation/explanation/disclaimer).")
                .HasColumnType("character varying");

            entity.HasOne(d => d.query).WithMany(p => p.AIResponses)
                .HasForeignKey(d => d.query_id)
                .HasConstraintName("AIResponses_query_id_fkey");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.message_id).HasName("ChatMessages_pkey");

            entity.ToTable(tb => tb.HasComment("Tin nhắn trong phiên trò chuyện."));

            entity.HasIndex(e => e.message_id, "idx_chatmessages_message_id");

            entity.HasIndex(e => e.session_id, "idx_chatmessages_session_id");

            entity.HasIndex(e => e.user_id, "idx_chatmessages_user_id");

            entity.Property(e => e.message_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính tin nhắn.");
            entity.Property(e => e.content).HasComment("Nội dung văn bản.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm gửi tin nhắn.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.image_url)
                .HasComment("URL ảnh đính kèm (nếu có).")
                .HasColumnType("character varying");
            entity.Property(e => e.message_type)
                .HasComment("Loại tin nhắn (text/image/mixed).")
                .HasColumnType("character varying");
            entity.Property(e => e.session_id).HasComment("Phiên chat chứa tin nhắn.");
            entity.Property(e => e.user_id).HasComment("Người gửi (user hoặc bot).");

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

            entity.ToTable(tb => tb.HasComment("Phiên trò chuyện giữa người dùng và AI."));

            entity.HasIndex(e => e.channel, "idx_chatsessions_channel");

            entity.HasIndex(e => new { e.channel, e.state }, "idx_chatsessions_channel_state");

            entity.HasIndex(e => e.session_id, "idx_chatsessions_session_id");

            entity.HasIndex(e => e.specialist_id, "idx_chatsessions_specialist_id");

            entity.HasIndex(e => e.state, "idx_chatsessions_state");

            entity.HasIndex(e => e.user_id, "idx_chatsessions_user_id");

            entity.HasIndex(e => e.created_at, "idx_cs_waiting_specialist").HasFilter("(((channel)::text = 'specialist'::text) AND ((state)::text = 'waiting_specialist'::text))");

            entity.Property(e => e.session_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính phiên chat.");
            entity.Property(e => e.assigned_at)
                .HasComment("Thời điểm assign specialist (áp dụng cho channel=specialist).")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.channel)
                .HasDefaultValueSql("'ai'::character varying")
                .HasComment("ai: user↔AI (templates-only); specialist: user↔specialist; ai_admin: admin/specialist↔AI (builder, có thể tạo routine mới).")
                .HasColumnType("character varying");
            entity.Property(e => e.closed_at)
                .HasComment("Thời điểm đóng phiên (tuỳ workflow).")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo phiên.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.specialist_id).HasComment("User_id của specialist đang phụ trách phiên (NULL nếu chưa gán/không áp dụng).");
            entity.Property(e => e.state)
                .HasDefaultValueSql("'open'::character varying")
                .HasComment("Trạng thái phiên chat:\n- open: Phiên đang mở (AI hoặc admin-special use-case), chưa cần specialist.\n- waiting_specialist: Phiên kênh specialist chưa có ai nhận (specialist_id IS NULL).\n- assigned: Phiên kênh specialist đã được gán cho 1 specialist (specialist_id NOT NULL), chưa đóng.\n- closed: Phiên đã đóng (closed_at NOT NULL).")
                .HasColumnType("character varying");
            entity.Property(e => e.title)
                .HasComment("Tiêu đề phiên chat.")
                .HasColumnType("character varying");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm cập nhật phiên.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.user_id).HasComment("Chủ sở hữu phiên chat.");

            entity.HasOne(d => d.specialist).WithMany(p => p.ChatSessionspecialists)
                .HasForeignKey(d => d.specialist_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_chatsessions_specialist");

            entity.HasOne(d => d.user).WithMany(p => p.ChatSessionusers)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("ChatSessions_user_id_fkey");
        });

        modelBuilder.Entity<ConsentRecord>(entity =>
        {
            entity.HasKey(e => e.consent_id).HasName("ConsentRecords_pkey");

            entity.ToTable(tb => tb.HasComment("Bản ghi đồng ý của người dùng."));

            entity.HasIndex(e => e.consent_id, "idx_consentrecords_consent_id");

            entity.HasIndex(e => e.user_id, "idx_consentrecords_user_id");

            entity.Property(e => e.consent_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính đồng ý.");
            entity.Property(e => e.consent_text).HasComment("Nội dung đồng ý.");
            entity.Property(e => e.consent_type)
                .HasComment("Loại đồng ý.")
                .HasColumnType("character varying");
            entity.Property(e => e.given).HasComment("Trạng thái đồng ý.");
            entity.Property(e => e.given_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm xác nhận.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.user_id).HasComment("Người dùng cho phép.");

            entity.HasOne(d => d.user).WithMany(p => p.ConsentRecords)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("ConsentRecords_user_id_fkey");
        });

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.HasKey(e => e.chunk_id).HasName("DocumentChunks_pkey");

            entity.ToTable(tb => tb.HasComment("Các đoạn văn bản đã chia nhỏ từ tài liệu."));

            entity.HasIndex(e => e.chunk_id, "idx_documentchunks_chunk_id");

            entity.HasIndex(e => e.created_at, "idx_documentchunks_created_at");

            entity.HasIndex(e => e.doc_id, "idx_documentchunks_doc_id");

            entity.HasIndex(e => e.embedding, "idx_documentchunks_embedding")
                .HasMethod("hnsw")
                .HasOperators(new[] { "vector_cosine_ops" });

            entity.Property(e => e.chunk_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính đoạn văn.");
            entity.Property(e => e.chunk_text).HasComment("Nội dung đoạn.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo đoạn.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.doc_id).HasComment("Tài liệu gốc chứa đoạn văn.");
            entity.Property(e => e.embedding)
                .HasMaxLength(1536)
                .HasComment("Vector(1536) dùng cho tìm kiếm ngữ nghĩa.");

            entity.HasOne(d => d.doc).WithMany(p => p.DocumentChunks)
                .HasForeignKey(d => d.doc_id)
                .HasConstraintName("DocumentChunks_doc_id_fkey");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.feedback_id).HasName("Feedback_pkey");

            entity.ToTable("Feedback", tb => tb.HasComment("Phản hồi của người dùng về liệu trình/bước."));

            entity.HasIndex(e => e.feedback_id, "idx_feedback_feedback_id");

            entity.HasIndex(e => e.routine_id, "idx_feedback_routine_id");

            entity.HasIndex(e => e.step_id, "idx_feedback_step_id");

            entity.HasIndex(e => e.user_id, "idx_feedback_user_id");

            entity.Property(e => e.feedback_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính phản hồi.");
            entity.Property(e => e.comment).HasComment("Nội dung phản hồi.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo phản hồi.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.rating).HasComment("Điểm đánh giá (1-5).");
            entity.Property(e => e.routine_id).HasComment("Liệu trình được phản hồi.");
            entity.Property(e => e.step_id).HasComment("Bước liên quan (nếu có).");
            entity.Property(e => e.user_id).HasComment("Người dùng phản hồi.");

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

            entity.ToTable(tb => tb.HasComment("Tài liệu tri thức y khoa phục vụ RAG."));

            entity.HasIndex(e => e.created_at, "idx_medicaldocuments_created_at");

            entity.HasIndex(e => e.doc_id, "idx_medicaldocuments_doc_id");

            entity.Property(e => e.doc_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính tài liệu.");
            entity.Property(e => e.content).HasComment("Nội dung tài liệu.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo tài liệu.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.last_updated)
                .HasComment("Thời điểm cập nhật nội dung gần nhất.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.source)
                .HasComment("Nguồn trích dẫn tài liệu.")
                .HasColumnType("character varying");
            entity.Property(e => e.status)
                .HasComment("Trạng thái hiển thị tài liệu.")
                .HasColumnType("character varying");
            entity.Property(e => e.title)
                .HasComment("Tiêu đề tài liệu.")
                .HasColumnType("character varying");
        });

        modelBuilder.Entity<MedicalDocumentAsset>(entity =>
        {
            entity.HasKey(e => e.asset_id).HasName("MedicalDocumentAssets_pkey");

            entity.ToTable(tb => tb.HasComment("Tập hợp ảnh/tệp đính kèm cho tài liệu y khoa."));

            entity.HasIndex(e => e.asset_id, "idx_medicaldocumentassets_asset_id");

            entity.HasIndex(e => e.doc_id, "idx_medicaldocumentassets_doc_id");

            entity.HasIndex(e => e.public_id, "idx_medicaldocumentassets_public_id");

            entity.Property(e => e.asset_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính bản ghi tài nguyên.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm lưu tài nguyên.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.doc_id).HasComment("Tài liệu y khoa sở hữu ảnh.");
            entity.Property(e => e.file_url).HasComment("URL công khai của ảnh/tệp.");
            entity.Property(e => e.height).HasComment("Chiều cao ảnh (pixel).");
            entity.Property(e => e.mime_type)
                .HasComment("Loại MIME của tài nguyên.")
                .HasColumnType("character varying");
            entity.Property(e => e.provider)
                .HasDefaultValueSql("'cloudinary'::character varying")
                .HasComment("Tên nhà cung cấp lưu trữ (mặc định Cloudinary).")
                .HasColumnType("character varying");
            entity.Property(e => e.public_id)
                .HasComment("Public ID từ Cloudinary (hoặc provider).")
                .HasColumnType("character varying");
            entity.Property(e => e.size_bytes).HasComment("Dung lượng tệp (byte).");
            entity.Property(e => e.width).HasComment("Chiều rộng ảnh (pixel).");

            entity.HasOne(d => d.doc).WithMany(p => p.MedicalDocumentAssets)
                .HasForeignKey(d => d.doc_id)
                .HasConstraintName("MedicalDocumentAssets_doc_id_fkey");
        });

        modelBuilder.Entity<QueryMatch>(entity =>
        {
            entity.HasKey(e => e.match_id).HasName("QueryMatches_pkey");

            entity.ToTable(tb => tb.HasComment("Kết quả đối sánh đoạn văn cho truy vấn."));

            entity.HasIndex(e => e.chunk_id, "idx_querymatches_chunk_id");

            entity.HasIndex(e => e.match_id, "idx_querymatches_match_id");

            entity.HasIndex(e => e.query_id, "idx_querymatches_query_id");

            entity.Property(e => e.match_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính bản ghi đối sánh.");
            entity.Property(e => e.chunk_id).HasComment("Đoạn văn được đối sánh.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo bản ghi.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.query_id).HasComment("Truy vấn liên quan.");
            entity.Property(e => e.similarity_score).HasComment("Điểm tương đồng (0-1).");

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

            entity.ToTable(tb => tb.HasComment("Các câu hỏi khảo sát người dùng."));

            entity.HasIndex(e => e.question_id, "idx_questions_question_id");

            entity.Property(e => e.question_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính câu hỏi.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo câu hỏi.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.options)
                .HasComment("Danh sách lựa chọn (JSON).")
                .HasColumnType("jsonb");
            entity.Property(e => e.text)
                .HasComment("Nội dung câu hỏi.")
                .HasColumnType("character varying");
            entity.Property(e => e.type)
                .HasComment("Loại câu hỏi (choice/multi-choice/text).")
                .HasColumnType("character varying");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.role_id).HasName("Roles_pkey");

            entity.ToTable(tb => tb.HasComment("Danh mục vai trò người dùng."));

            entity.HasIndex(e => e.name, "Roles_name_key").IsUnique();

            entity.HasIndex(e => e.name, "idx_roles_name").IsUnique();

            entity.HasIndex(e => e.role_id, "idx_roles_role_id");

            entity.Property(e => e.role_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính vai trò.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo vai trò.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.description).HasComment("Mô tả vai trò.");
            entity.Property(e => e.name)
                .HasComment("Tên vai trò duy nhất.")
                .HasColumnType("character varying");
            entity.Property(e => e.status)
                .HasComment("Trạng thái hoạt động của vai trò.")
                .HasColumnType("character varying");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm cập nhật gần nhất.")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<Routine>(entity =>
        {
            entity.HasKey(e => e.routine_id).HasName("Routines_pkey");

            entity.ToTable(tb => tb.HasComment("Liệu trình chăm sóc da do AI đề xuất."));

            entity.HasIndex(e => e.analysis_id, "idx_routines_analysis_id");

            entity.HasIndex(e => e.routine_id, "idx_routines_routine_id");

            entity.HasIndex(e => e.user_id, "idx_routines_user_id");

            entity.Property(e => e.routine_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính liệu trình.");
            entity.Property(e => e.analysis_id).HasComment("Phân tích AI liên quan.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo liệu trình.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.description).HasComment("Mô tả chung liệu trình.");
            entity.Property(e => e.parent_routine_id).HasComment("Liệu trình cha (nếu có).");
            entity.Property(e => e.routine_type)
                .HasDefaultValueSql("'template'::character varying")
                .HasColumnType("character varying");
            entity.Property(e => e.status)
                .HasComment("Trạng thái liệu trình.")
                .HasColumnType("character varying");
            entity.Property(e => e.target_skin_type).HasColumnType("character varying");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm cập nhật liệu trình.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.user_id).HasComment("Người dùng sở hữu liệu trình.");
            entity.Property(e => e.version)
                .HasDefaultValue(1)
                .HasComment("Phiên bản liệu trình.");

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

            entity.ToTable(tb => tb.HasComment("Lần triển khai liệu trình theo thời gian."));

            entity.HasIndex(e => e.instance_id, "idx_routineinstances_instance_id");

            entity.HasIndex(e => e.routine_id, "idx_routineinstances_routine_id");

            entity.HasIndex(e => e.user_id, "idx_routineinstances_user_id");

            entity.Property(e => e.instance_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính instance.");
            entity.Property(e => e.adherence_score).HasPrecision(5, 2);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo instance.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.end_date).HasComment("Ngày kết thúc (nếu có).");
            entity.Property(e => e.routine_id).HasComment("Liệu trình triển khai.");
            entity.Property(e => e.start_date).HasComment("Ngày bắt đầu.");
            entity.Property(e => e.status)
                .HasComment("Trạng thái thực hiện.")
                .HasColumnType("character varying");
            entity.Property(e => e.user_id).HasComment("Người dùng áp dụng.");

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

            entity.ToTable("RoutineProgress", tb => tb.HasComment("Tiến độ thực hiện từng bước."));

            entity.HasIndex(e => e.instance_id, "idx_routineprogress_instance_id");

            entity.HasIndex(e => e.progress_id, "idx_routineprogress_progress_id");

            entity.HasIndex(e => e.step_id, "idx_routineprogress_step_id");

            entity.Property(e => e.progress_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính tiến độ.");
            entity.Property(e => e.completed_at)
                .HasComment("Thời điểm hoàn tất.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.instance_id).HasComment("Instance liên quan.");
            entity.Property(e => e.mood_note).HasColumnType("character varying");
            entity.Property(e => e.note).HasComment("Ghi chú thêm.");
            entity.Property(e => e.photo_url)
                .HasComment("Ảnh minh chứng (nếu có).")
                .HasColumnType("character varying");
            entity.Property(e => e.status)
                .HasDefaultValueSql("'completed'::character varying")
                .HasComment("Trạng thái tiến độ.")
                .HasColumnType("character varying");
            entity.Property(e => e.step_id).HasComment("Bước được ghi nhận.");

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

            entity.ToTable(tb => tb.HasComment("Các bước cụ thể trong liệu trình."));

            entity.HasIndex(e => e.routine_id, "idx_routinesteps_routine_id");

            entity.HasIndex(e => e.step_id, "idx_routinesteps_step_id");

            entity.Property(e => e.step_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính bước liệu trình.");
            entity.Property(e => e.frequency)
                .HasDefaultValueSql("'daily'::character varying")
                .HasComment("Tần suất thực hiện.")
                .HasColumnType("character varying");
            entity.Property(e => e.instruction).HasComment("Hướng dẫn chi tiết.");
            entity.Property(e => e.routine_id).HasComment("Liệu trình chứa bước này.");
            entity.Property(e => e.step_order).HasComment("Thứ tự thực hiện bước.");
            entity.Property(e => e.time_of_day)
                .HasComment("Thời điểm trong ngày.")
                .HasColumnType("character varying");

            entity.HasOne(d => d.routine).WithMany(p => p.RoutineSteps)
                .HasForeignKey(d => d.routine_id)
                .HasConstraintName("RoutineSteps_routine_id_fkey");
        });

        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(e => e.rule_id).HasName("Rules_pkey");

            entity.ToTable(tb => tb.HasComment("Tập luật gợi ý/skincare."));

            entity.HasIndex(e => e.rule_id, "idx_rules_rule_id");

            entity.Property(e => e.rule_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính luật.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo luật.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.recommendation).HasComment("Khuyến nghị tương ứng.");
            entity.Property(e => e.urgency_level)
                .HasComment("Mức độ khẩn cấp của luật.")
                .HasColumnType("character varying");
        });

        modelBuilder.Entity<RuleCondition>(entity =>
        {
            entity.HasKey(e => e.rule_condition_id).HasName("RuleConditions_pkey");

            entity.ToTable(tb => tb.HasComment("Điều kiện áp dụng cho mỗi luật."));

            entity.HasIndex(e => e.question_id, "idx_ruleconditions_question_id");

            entity.HasIndex(e => e.rule_condition_id, "idx_ruleconditions_rule_condition_id");

            entity.HasIndex(e => e.rule_id, "idx_ruleconditions_rule_id");

            entity.HasIndex(e => e.symptom_id, "idx_ruleconditions_symptom_id");

            entity.Property(e => e.rule_condition_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính điều kiện.");
            entity.Property(e => e._operator)
                .HasComment("Toán tử so sánh.")
                .HasColumnType("character varying")
                .HasColumnName("operator");
            entity.Property(e => e.question_id).HasComment("Câu hỏi điều kiện.");
            entity.Property(e => e.rule_id).HasComment("Luật được áp dụng.");
            entity.Property(e => e.symptom_id).HasComment("Triệu chứng điều kiện.");
            entity.Property(e => e.value).HasComment("Giá trị so sánh.");

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

            entity.ToTable(tb => tb.HasComment("Danh sách triệu chứng da liễu."));

            entity.HasIndex(e => e.name, "Symptoms_name_key").IsUnique();

            entity.HasIndex(e => e.name, "idx_symptoms_name").IsUnique();

            entity.HasIndex(e => e.symptom_id, "idx_symptoms_symptom_id");

            entity.Property(e => e.symptom_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính triệu chứng.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo triệu chứng.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.description).HasComment("Mô tả chi tiết.");
            entity.Property(e => e.example_image_url)
                .HasComment("Ảnh minh họa.")
                .HasColumnType("character varying");
            entity.Property(e => e.name)
                .HasComment("Tên triệu chứng duy nhất.")
                .HasColumnType("character varying");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("Users_pkey");

            entity.ToTable(tb => tb.HasComment("Thông tin người dùng hệ thống."));

            entity.HasIndex(e => e.email, "Users_email_key").IsUnique();

            entity.HasIndex(e => e.google_id, "Users_google_id_key").IsUnique();

            entity.HasIndex(e => e.email, "idx_users_email").IsUnique();

            entity.HasIndex(e => e.role_id, "idx_users_role_id");

            entity.HasIndex(e => e.user_id, "idx_users_user_id");

            entity.Property(e => e.user_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính người dùng.");
            entity.Property(e => e.auth_provider)
                .HasDefaultValueSql("'google'::character varying")
                .HasColumnType("character varying");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo người dùng.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.date_of_birth).HasComment("Ngày sinh của người dùng.");
            entity.Property(e => e.email)
                .HasComment("Email đăng nhập duy nhất.")
                .HasColumnType("character varying");
            entity.Property(e => e.full_name)
                .HasComment("Họ và tên đầy đủ.")
                .HasColumnType("character varying");
            entity.Property(e => e.google_id)
                .HasComment("ID Google dùng để xác thực.")
                .HasColumnType("character varying");
            entity.Property(e => e.password_hash).HasColumnType("character varying");
            entity.Property(e => e.role_id).HasComment("Vai trò của người dùng.");
            entity.Property(e => e.skin_type)
                .HasComment("Loại da của người dùng.")
                .HasColumnType("character varying");
            entity.Property(e => e.status)
                .HasComment("Trạng thái người dùng (active/inactive/banned).")
                .HasColumnType("character varying");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm cập nhật người dùng.")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.role).WithMany(p => p.Users)
                .HasForeignKey(d => d.role_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Users_role_id_fkey");
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.answer_id).HasName("UserAnswers_pkey");

            entity.ToTable(tb => tb.HasComment("Câu trả lời khảo sát của người dùng."));

            entity.HasIndex(e => e.answer_id, "idx_useranswers_answer_id");

            entity.HasIndex(e => e.question_id, "idx_useranswers_question_id");

            entity.HasIndex(e => e.user_id, "idx_useranswers_user_id");

            entity.Property(e => e.answer_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính câu trả lời.");
            entity.Property(e => e.answer_value).HasComment("Giá trị câu trả lời.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo câu trả lời.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.question_id).HasComment("Câu hỏi được trả lời.");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm cập nhật câu trả lời.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.user_id).HasComment("Người dùng thực hiện trả lời.");

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

            entity.ToTable(tb => tb.HasComment("Lịch sử truy vấn của người dùng."));

            entity.HasIndex(e => e.created_at, "idx_userqueries_created_at");

            entity.HasIndex(e => e.query_embedding, "idx_userqueries_query_embedding")
                .HasMethod("hnsw")
                .HasOperators(new[] { "vector_cosine_ops" });

            entity.HasIndex(e => e.query_id, "idx_userqueries_query_id");

            entity.HasIndex(e => e.user_id, "idx_userqueries_user_id");

            entity.Property(e => e.query_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính truy vấn.");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm tạo truy vấn.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.query_embedding)
                .HasMaxLength(1536)
                .HasComment("Vector(1536) biểu diễn truy vấn.");
            entity.Property(e => e.query_text).HasComment("Nội dung câu hỏi.");
            entity.Property(e => e.user_id).HasComment("Người dùng thực hiện truy vấn.");

            entity.HasOne(d => d.user).WithMany(p => p.UserQueries)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("UserQueries_user_id_fkey");
        });

        modelBuilder.Entity<UserSymptom>(entity =>
        {
            entity.HasKey(e => e.user_symptom_id).HasName("UserSymptoms_pkey");

            entity.ToTable(tb => tb.HasComment("Ghi nhận triệu chứng người dùng gặp phải."));

            entity.HasIndex(e => e.symptom_id, "idx_usersymptoms_symptom_id");

            entity.HasIndex(e => e.user_id, "idx_usersymptoms_user_id");

            entity.HasIndex(e => e.user_symptom_id, "idx_usersymptoms_user_symptom_id");

            entity.Property(e => e.user_symptom_id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Khóa chính bản ghi triệu chứng.");
            entity.Property(e => e.reported_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời điểm báo cáo triệu chứng.")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.symptom_id).HasComment("Triệu chứng đã chọn.");
            entity.Property(e => e.user_id).HasComment("Người dùng liên quan.");

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
