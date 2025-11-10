using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using SkinCareSystem.Repositories.IRepositories;

namespace SkinCareSystem.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        IUserRepository UserRepository { get; }
        IRoleRepository RoleRepository { get; }
        IChatSessionRepository ChatSessionRepository { get; }
        IChatMessageRepository ChatMessageRepository { get; }
        IAIAnalysisRepository AIAnalysisRepository { get; }
        IAIResponseRepository AIResponseRepository { get; }
        IUserQueryRepository UserQueryRepository { get; }
        IRoutineRepository RoutineRepository { get; }
        IRoutineStepRepository RoutineStepRepository { get; }
        IRoutineInstanceRepository RoutineInstanceRepository { get; }
        IRoutineProgressRepository RoutineProgressRepository { get; }
        IFeedbackRepository FeedbackRepository { get; }
        IRuleRepository RuleRepository { get; }
        IRuleConditionRepository RuleConditionRepository { get; }
        ISymptomRepository SymptomRepository { get; }
        IUserSymptomRepository UserSymptomRepository { get; }
        IQuestionRepository Questions { get; }
        IUserAnswerRepository UserAnswers { get; }
        IMedicalDocumentRepository MedicalDocuments { get; }
        IMedicalDocumentAssetRepository MedicalDocumentAssets { get; }
        IDocumentChunkRepository DocumentChunks { get; }
        IConsentRecordRepository ConsentRecords { get; }
        Task<int> SaveAsync();
        int Save();
    }
}
