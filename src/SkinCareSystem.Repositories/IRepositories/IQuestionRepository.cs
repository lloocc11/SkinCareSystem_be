using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.IRepositories
{
    public interface IQuestionRepository : IGenericRepository<Question>
    {
        Task<Question?> GetByIdWithDetailsAsync(Guid questionId);
    }

    public interface IUserAnswerRepository : IGenericRepository<UserAnswer>
    {
        Task<IReadOnlyList<UserAnswer>> GetByUserIdAsync(Guid userId);
        Task<UserAnswer?> GetByIdWithDetailsAsync(Guid answerId);
    }
}
