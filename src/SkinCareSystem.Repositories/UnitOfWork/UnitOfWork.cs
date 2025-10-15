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
