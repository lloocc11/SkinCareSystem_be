using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkinCareSystem.Repositories.Base
{
    public interface IGenericRepository<T> where T : class
    {
        List<T> GetAll();
        Task<List<T>> GetAllAsync();
        void Create(T entity);
        Task CreateAsync(T entity);
        void Update(T entity);
        Task UpdateAsync(T entity);
        void Remove(T entity);
        Task RemoveAsync(T entity);
        T GetById(int id);
        Task<T?> GetByIdAsync(int id);
        T GetById(string code);
        Task<T?> GetByIdAsync(string code);
        T GetById(Guid code);
        Task<T?> GetByIdAsync(Guid code);
        IQueryable<T> GetAllQueryable();
    }
}
