using SmartTutor.DataAccess.Persistence;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.DataAccess.Repositories.Repo
{
    public class MonthlyReportRepository : BaseRepository<MonthlyReport>, IMonthlyReportRepository
    {
        public MonthlyReportRepository(SmartTutorContext context) : base(context)
        {
        }
    }
}
