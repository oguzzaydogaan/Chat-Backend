using Microsoft.EntityFrameworkCore;
using Repositories.Context;
using Repositories.Entities;

namespace Repositories.Repositories
{
    public class CallRepository : BaseRepository<Call>
    {
        public CallRepository(RepositoryContext context)
            : base(context)
        {
        }

        public async Task<Call?> GetActiveByUserIdAsync(int userId)
        {
            return await DbSet.FirstOrDefaultAsync(c => (c.CallerId == userId || c.Callees.Any(u=>u.Id==userId)) && (c.AnswerType == CallAnswerType.None || (c.AnswerType == CallAnswerType.Accepted && c.EndTime == DateTime.MinValue)));
        }

        public async Task<List<Call>> GetNotActivesByUserIdAsync(int userId)
        {
            return await DbSet.Include(c=>c.Callees).Include(c=>c.Caller).Include(c=>c.Chat).Where(c => c.Callees.Count == 1 && (c.CallerId == userId || c.Callees.Any(u => u.Id == userId)) && (c.AnswerType == CallAnswerType.Cancelled || c.AnswerType == CallAnswerType.Rejected || (c.AnswerType == CallAnswerType.Accepted && c.EndTime != DateTime.MinValue))).OrderByDescending(c=>c.CallTime).ToListAsync();
        }

        public async Task<Call?> GetByIdWithCalleesAsync(int id)
        {
            return await DbSet.Include(c=>c.Callees).FirstAsync(c=>c.Id==id);
        }
    }
}
