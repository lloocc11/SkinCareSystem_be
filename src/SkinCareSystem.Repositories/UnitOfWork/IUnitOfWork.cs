using Microsoft.EntityFrameworkCore.Storage;
using SkinCareSystem;
using SkinCareSystem.Repositories.IRepositores;

namespace SkinCareSystem.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        IUserRepository UserRepository { get; }
        Task<int> SaveAsync();
        int Save();
    }
}
