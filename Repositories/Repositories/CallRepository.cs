using Exceptions;
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
            return await DbSet.FirstOrDefaultAsync(c => (c.CallerId == userId || c.CalleeId == userId) && (c.AnswerType == CallAnswerType.None || (c.AnswerType == CallAnswerType.Accepted && c.EndTime == DateTime.MinValue)));
        }
    }
}
