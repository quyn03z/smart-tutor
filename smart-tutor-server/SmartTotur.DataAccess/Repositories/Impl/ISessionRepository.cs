using SmartTutor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.DataAccess.Repositories.Impl
{
    public interface ISessionRepository : IBaseRepository<Session>
    {
        Task<IEnumerable<Session>> GetMyTeacherSessionsAsync(DateOnly? fromDate, DateOnly? toDate, int? userId);
    }
}
