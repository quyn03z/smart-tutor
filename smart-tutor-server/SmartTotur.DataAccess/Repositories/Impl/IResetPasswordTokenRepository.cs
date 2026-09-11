using SmartTutor.Domain.Models;
using System;
using System.Threading.Tasks;

namespace SmartTutor.DataAccess.Repositories.Impl
{
    public interface IResetPasswordTokenRepository : IBaseRepository<ResetPasswordToken>
    {
        Task<ResetPasswordToken?> GetValidTokenAsync(string token);
        Task InvalidateTokensByUserIdAsync(int userId);
    }
}
