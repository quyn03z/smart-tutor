using System;
using System.Collections.Generic;
using System.Text;
using static SmartTutor.BusinessLogic.Models.SessionModels;

namespace SmartTutor.BusinessLogic.Services.Impl
{
    public interface ISessionService
    {
        Task<IEnumerable<SessionRespondModel>> GetMyTeacherSessionsAsync(DateOnly? fromDate, DateOnly? toDate);
        Task<SessionRespondModel> CreateSessionsAsync(SessionRequestModel sessionRequestModel);
        Task<SessionRespondModel> EditSessionsAsync(SessionRequestModel sessionRequestModel);
        Task<string> DeleteSessionAsync(int sessionId);
    }
}
