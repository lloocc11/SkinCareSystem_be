using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.IRepositories;
using SkinCareSystem.Repositories.Repositories;

namespace SkinCareSystem.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SkinCareSystemDbContext _context;
        private IUserRepository? userRepository;
        private IRoleRepository? roleRepository;
        private IChatSessionRepository? chatSessionRepository;
        private IChatMessageRepository? chatMessageRepository;
        private IAIAnalysisRepository? aiAnalysisRepository;
        private IAIResponseRepository? aiResponseRepository;
        private IUserQueryRepository? userQueryRepository;
        private IRoutineRepository? routineRepository;
        private IRoutineStepRepository? routineStepRepository;
        private IRoutineInstanceRepository? routineInstanceRepository;
        private IRoutineProgressRepository? routineProgressRepository;
        private IFeedbackRepository? feedbackRepository;
        private IRuleRepository? ruleRepository;
        private IRuleConditionRepository? ruleConditionRepository;
        private ISymptomRepository? symptomRepository;
        private IUserSymptomRepository? userSymptomRepository;
        private IQuestionRepository? questionRepository;
        private IUserAnswerRepository? userAnswerRepository;
        private IMedicalDocumentRepository? medicalDocumentRepository;
        private IMedicalDocumentAssetRepository? medicalDocumentAssetRepository;
        private IDocumentChunkRepository? documentChunkRepository;
        private IConsentRecordRepository? consentRecordRepository;
        
        public UnitOfWork(SkinCareSystemDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
        
        public IUserRepository UserRepository
        {
            get
            {
                return userRepository ??= new UserRepository(_context);
            }
        }

        public IRoleRepository RoleRepository
        {
            get
            {
                return roleRepository ??= new RoleRepository(_context);
            }
        }

        public IChatSessionRepository ChatSessionRepository
        {
            get
            {
                return chatSessionRepository ??= new ChatSessionRepository(_context);
            }
        }

        public IChatMessageRepository ChatMessageRepository
        {
            get
            {
                return chatMessageRepository ??= new ChatMessageRepository(_context);
            }
        }

        public IAIAnalysisRepository AIAnalysisRepository
        {
            get
            {
                return aiAnalysisRepository ??= new AIAnalysisRepository(_context);
            }
        }

        public IAIResponseRepository AIResponseRepository
        {
            get
            {
                return aiResponseRepository ??= new AIResponseRepository(_context);
            }
        }

        public IUserQueryRepository UserQueryRepository
        {
            get
            {
                return userQueryRepository ??= new UserQueryRepository(_context);
            }
        }

        public IRoutineRepository RoutineRepository
        {
            get
            {
                return routineRepository ??= new RoutineRepository(_context);
            }
        }

        public IRoutineStepRepository RoutineStepRepository
        {
            get
            {
                return routineStepRepository ??= new RoutineStepRepository(_context);
            }
        }

        public IRoutineInstanceRepository RoutineInstanceRepository
        {
            get
            {
                return routineInstanceRepository ??= new RoutineInstanceRepository(_context);
            }
        }

        public IRoutineProgressRepository RoutineProgressRepository
        {
            get
            {
                return routineProgressRepository ??= new RoutineProgressRepository(_context);
            }
        }

        public IFeedbackRepository FeedbackRepository
        {
            get
            {
                return feedbackRepository ??= new FeedbackRepository(_context);
            }
        }

        public IRuleRepository RuleRepository
        {
            get
            {
                return ruleRepository ??= new RuleRepository(_context);
            }
        }

        public IRuleConditionRepository RuleConditionRepository
        {
            get
            {
                return ruleConditionRepository ??= new RuleConditionRepository(_context);
            }
        }

        public ISymptomRepository SymptomRepository
        {
            get
            {
                return symptomRepository ??= new SymptomRepository(_context);
            }
        }

        public IUserSymptomRepository UserSymptomRepository
        {
            get
            {
                return userSymptomRepository ??= new UserSymptomRepository(_context);
            }
        }

        public IQuestionRepository Questions
        {
            get
            {
                return questionRepository ??= new QuestionRepository(_context);
            }
        }

        public IUserAnswerRepository UserAnswers
        {
            get
            {
                return userAnswerRepository ??= new UserAnswerRepository(_context);
            }
        }

        public IMedicalDocumentRepository MedicalDocuments
        {
            get
            {
                return medicalDocumentRepository ??= new MedicalDocumentRepository(_context);
            }
        }

        public IMedicalDocumentAssetRepository MedicalDocumentAssets
        {
            get
            {
                return medicalDocumentAssetRepository ??= new MedicalDocumentAssetRepository(_context);
            }
        }

        public IDocumentChunkRepository DocumentChunks
        {
            get
            {
                return documentChunkRepository ??= new DocumentChunkRepository(_context);
            }
        }

        public IConsentRecordRepository ConsentRecords
        {
            get
            {
                return consentRecordRepository ??= new ConsentRecordRepository(_context);
            }
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

    }
}
