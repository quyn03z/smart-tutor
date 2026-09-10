using SmartTutor.Domain.Models;
using System;
using System.Threading.Tasks;

namespace SmartTutor.DataAccess.Repositories.Impl
{
    public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task RevokeTokensByUserIdAsync(int userId);
    }
}
