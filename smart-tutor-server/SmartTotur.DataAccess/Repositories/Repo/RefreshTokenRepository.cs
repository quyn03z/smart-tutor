using Microsoft.EntityFrameworkCore;
using SmartTutor.DataAccess.Persistence;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.Domain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTutor.DataAccess.Repositories.Repo
{
    public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(SmartTutorContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .Include(x => x.User)
                .ThenInclude(u => u!.Role)
                .FirstOrDefaultAsync(x => x.Token == token && !x.IsRevoked);
        }

        public async Task RevokeTokensByUserIdAsync(int userId)
        {
            var activeTokens = await _dbSet
                .Where(x => x.UserId == userId && !x.IsRevoked)
                .ToListAsync();

            if (activeTokens.Count > 0)
            {
                foreach (var token in activeTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
