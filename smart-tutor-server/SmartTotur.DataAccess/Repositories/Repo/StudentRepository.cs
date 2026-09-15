using Microsoft.EntityFrameworkCore;
using SmartTutor.DataAccess.Persistence;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.DataAccess.Repositories.Repo
{
    public class StudentRepository : BaseRepository<Student>, IStudentRepository
    {
        public StudentRepository(SmartTutorContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Student>> GetAllStudentByUserId(int userId)
        {
            return await _dbSet.Include(c => c.ClassEnrollments)
                                .ThenInclude(c => c.Class)
                                .Where(s => s.UserId == userId & s.Status != "Deleted")
                                .ToListAsync();
        }

        public async Task<Student> GetStudentByStudentId(int studentId)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.Id == studentId);
        }

        public async Task<Student> GetStudentDetailAsync(int studentId)
        {
            return await _dbSet
                        .Include(s => s.ClassEnrollments)
                            .ThenInclude(ce => ce.Class)
                        .Include(s => s.AttendanceLogs)
                            .ThenInclude(al => al.Session)
                        .FirstOrDefaultAsync(s => s.Id == studentId);
        }

        public async Task<Student?> GetStudentWithCreditHistoryAsync(int studentId)
        {
            return await _dbSet
                .Include(s => s.MonthlyReports)
                    .ThenInclude(mr => mr.Class)
                .Include(s => s.MonthlyReports)
                    .ThenInclude(mr => mr.PaymentTransactions)
                .Include(s => s.PaymentTransactions)
                .FirstOrDefaultAsync(s => s.Id == studentId);
        }
    }
}
