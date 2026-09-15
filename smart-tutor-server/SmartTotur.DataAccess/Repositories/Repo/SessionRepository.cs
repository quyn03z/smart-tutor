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
    }
}
