using Microsoft.EntityFrameworkCore;
using SmartTutor.DataAccess.Persistence;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.DataAccess.Repositories.Repo
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(SmartTutorContext context) : base(context)
        {
        }
         
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Email == email); 
        }
           
        public async Task<bool> IsEmailExistAsync(string email)
        {
            return await _dbSet.AnyAsync(x => x.Email == email);
        }
    }
}
