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
        Task<int> SaveAsync();
        int Save();
    }
}
