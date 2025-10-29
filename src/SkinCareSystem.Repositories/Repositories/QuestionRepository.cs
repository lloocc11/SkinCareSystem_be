using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.IRepositories;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.Repositories
{
    public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
    {
        public QuestionRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<Question?> GetByIdWithDetailsAsync(Guid questionId)
        {
            return await _context.Set<Question>()
                .AsNoTracking()
                .Include(q => q.UserAnswers)
                .FirstOrDefaultAsync(q => q.question_id == questionId);
        }
    }

    public class UserAnswerRepository : GenericRepository<UserAnswer>, IUserAnswerRepository
    {
        public UserAnswerRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<UserAnswer>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Set<UserAnswer>()
                .AsNoTracking()
                .Include(ua => ua.user)
                .Include(ua => ua.question)
                .Where(ua => ua.user_id == userId)
                .OrderByDescending(ua => ua.created_at)
                .ToListAsync();
        }

        public async Task<UserAnswer?> GetByIdWithDetailsAsync(Guid answerId)
        {
            return await _context.Set<UserAnswer>()
                .AsNoTracking()
                .Include(ua => ua.user)
                .Include(ua => ua.question)
                .FirstOrDefaultAsync(ua => ua.answer_id == answerId);
        }
    }
}
