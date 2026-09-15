using SmartTutor.BusinessLogic.Exceptions;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Claims;
using SmartTutor.DataAccess.Repositories.Impl;
using System;
using System.Collections.Generic;
using System.Text;
using static SmartTutor.BusinessLogic.Models.SessionModels;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IClaimService _claimService;

        public SessionService(ISessionRepository sessionRepository, IClaimService claimService)
        {
            _sessionRepository = sessionRepository;
            _claimService = claimService;
        }

        public async Task<IEnumerable<SessionRespondModel>> GetMyTeacherSessionsAsync(DateOnly? fromDate, DateOnly? toDate)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            var mySessions = await _sessionRepository.GetMyTeacherSessionsAsync(fromDate, toDate, userId.Value);

            return mySessions.Select(s => new SessionRespondModel
            {
                Id = s.Id,
                ClassId = s.ClassId,
                ClassName = s.Class?.ClassName,
                SessionDate = s.SessionDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                DurationHours = s.DurationHours,
                LessonContent = s.LessonContent,
                Status = s.Status
            });
        }
    }
}
