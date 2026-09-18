using Microsoft.EntityFrameworkCore;
using SmartTutor.DataAccess.Persistence;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.DataAccess.Repositories.Repo
{
    public class SessionRepository : BaseRepository<Session>, ISessionRepository
    {
        public SessionRepository(SmartTutorContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Session>> GetMyTeacherSessionsAsync(DateOnly? fromDate, DateOnly? toDate, int? userId)
        {
            var query = _dbSet.Include(c => c.Class)
                                .Include(a => a.AttendanceLogs)
                                .Where(x => x.Class.UserId == userId);
            if(fromDate.HasValue)
            {
                var fromDateTime = fromDate.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(x => x.SessionDate >= fromDateTime);
            }
            if (toDate.HasValue)
            {
                var toDateTime = toDate.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(x => x.SessionDate <= toDateTime);
            }
            return await query
                        .OrderBy(s => s.SessionDate)
                        .ThenBy(s => s.StartTime)
                        .ToListAsync();
        }

        public async Task<Session?> GetSessionWithAttendanceAsync(int sessionId)
        {
            return await _dbSet
                .Include(s => s.Class)
                    .ThenInclude(c => c!.ClassEnrollments)
                        .ThenInclude(ce => ce.Student)
                .Include(s => s.AttendanceLogs)
                    .ThenInclude(al => al.Student)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }
    }
}
