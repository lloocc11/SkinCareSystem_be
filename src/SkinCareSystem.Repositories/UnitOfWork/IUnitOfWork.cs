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
        Task<int> SaveAsync();
        int Save();
    }
}
