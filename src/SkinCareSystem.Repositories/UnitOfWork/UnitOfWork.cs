using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using SkinCareSystem.Repositories;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.Models;
using SkinCareSystem.Repositories.IRepositores;
using SkinCareSystem.Repositories.Repositories;

namespace SkinCareSystem.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SkinCareSystemDbContext _context;
        private IUserRepository? userRepository;
        
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