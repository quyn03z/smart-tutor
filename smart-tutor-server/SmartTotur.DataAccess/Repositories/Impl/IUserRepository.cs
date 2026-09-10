using SmartTutor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.DataAccess.Repositories.Impl
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<bool> IsEmailExistAsync(string email);
        Task<User?> GetUserByEmailAsync(string email);

    }
}
