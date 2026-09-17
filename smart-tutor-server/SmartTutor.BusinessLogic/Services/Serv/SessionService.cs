using SmartTutor.BusinessLogic.Exceptions;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Claims;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.Domain.Enums;
using SmartTutor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static SmartTutor.BusinessLogic.Models.SessionModels;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IClassRepository _classRepository;
        private readonly IClaimService _claimService;

        public SessionService(
            ISessionRepository sessionRepository, 
            IClassRepository classRepository,
            IClaimService claimService)
        {
            _sessionRepository = sessionRepository;
            _classRepository = classRepository;
            _claimService = claimService;
        }

        public async Task<SessionRespondModel> CreateSessionsAsync(SessionRequestModel sessionRequestModel)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            // 1. Kiểm tra lớp học có tồn tại và thuộc về giáo viên hiện tại không
            var @class = await _classRepository.GetByIdAsync(sessionRequestModel.ClassId);
            if (@class == null || @class.UserId != userId.Value)
                throw new NotFoundException("Không tìm thấy lớp học hoặc bạn không có quyền tạo ca học cho lớp này.");

            // 2. Tính toán thời lượng buổi học nếu chưa có
            var durationHours = sessionRequestModel.DurationHours;
            if (durationHours <= 0 && sessionRequestModel.EndTime > sessionRequestModel.StartTime)
            {
                durationHours = (decimal)(sessionRequestModel.EndTime - sessionRequestModel.StartTime).TotalHours;
            }

            // 3. Tạo Entity Session mới
            var session = new Session
            {
                ClassId = sessionRequestModel.ClassId,
                SessionDate = sessionRequestModel.SessionDate.Date,
                StartTime = sessionRequestModel.StartTime,
                EndTime = sessionRequestModel.EndTime,
                DurationHours = durationHours,
                LessonContent = sessionRequestModel.LessonContent,
                Status = string.IsNullOrWhiteSpace(sessionRequestModel.Status) 
                    ? AppEnums.SessionStatus.Scheduled.ToString() 
                    : sessionRequestModel.Status,
                CreatedAt = DateTime.UtcNow
            };

            var createdSession = await _sessionRepository.AddAsync(session);

            // 4. Trả về kết quả
            return new SessionRespondModel
            {
                Id = createdSession.Id,
                ClassId = createdSession.ClassId,
                ClassName = @class.ClassName,
                SessionDate = createdSession.SessionDate,
                StartTime = createdSession.StartTime,
                EndTime = createdSession.EndTime,
                DurationHours = createdSession.DurationHours,
                LessonContent = createdSession.LessonContent,
                Status = createdSession.Status
            };
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
