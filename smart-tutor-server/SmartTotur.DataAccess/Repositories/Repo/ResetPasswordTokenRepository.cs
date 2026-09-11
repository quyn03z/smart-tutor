using Microsoft.EntityFrameworkCore;
using SmartTutor.DataAccess.Persistence;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.Domain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTutor.DataAccess.Repositories.Repo
{
    public class ResetPasswordTokenRepository : BaseRepository<ResetPasswordToken>, IResetPasswordTokenRepository
    {
        public ResetPasswordTokenRepository(SmartTutorContext context) : base(context)
        {
        }

        public async Task<ResetPasswordToken?> GetValidTokenAsync(string token)
        {
            return await _dbSet
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.ResetToken == token && !x.isUsed && x.ExpiredAt > DateTime.UtcNow);
        }

        public async Task InvalidateTokensByUserIdAsync(int userId)
        {
            var activeTokens = await _dbSet
                .Where(x => x.UserId == userId && !x.isUsed)
                .ToListAsync();

            if (activeTokens.Count > 0)
            {
                foreach (var t in activeTokens)
                {
                    t.isUsed = true;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
