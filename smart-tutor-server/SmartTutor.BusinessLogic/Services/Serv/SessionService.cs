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

            // 4. Nếu có chọn lặp lại theo quy luật (Recurrence Pattern)
            var pattern = (sessionRequestModel.RecurrencePattern ?? "NONE").ToUpperInvariant();
            var count = sessionRequestModel.RecurringCount > 0 ? sessionRequestModel.RecurringCount : (sessionRequestModel.RecurringWeeks > 0 ? sessionRequestModel.RecurringWeeks : 4);

            if (pattern != "NONE" || (sessionRequestModel.IsRecurring && count > 1))
            {
                await GenerateRecurringSessionsAsync(sessionRequestModel, durationHours, count);
            }

            // 5. Trả về kết quả
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

        private async Task GenerateRecurringSessionsAsync(SessionRequestModel request, decimal durationHours, int count)
        {
            var pattern = (request.RecurrencePattern ?? "NONE").ToUpperInvariant();
            var startDate = request.SessionDate.Date;

            if (pattern == "DAILY")
            {
                int limit = Math.Min(count * 7, 30);
                for (int d = 1; d < limit; d++)
                {
                    var nextDate = startDate.AddDays(d);
                    var recurringSession = new Session
                    {
                        ClassId = request.ClassId,
                        SessionDate = nextDate,
                        StartTime = request.StartTime,
                        EndTime = request.EndTime,
                        DurationHours = durationHours,
                        LessonContent = request.LessonContent,
                        Status = AppEnums.SessionStatus.Scheduled.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _sessionRepository.AddAsync(recurringSession);
                }
            }
            else if (pattern == "WEEKDAYS")
            {
                int totalDays = count * 7;
                for (int d = 1; d <= totalDays; d++)
                {
                    var nextDate = startDate.AddDays(d);
                    if (nextDate.DayOfWeek != DayOfWeek.Saturday && nextDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        var recurringSession = new Session
                        {
                            ClassId = request.ClassId,
                            SessionDate = nextDate,
                            StartTime = request.StartTime,
                            EndTime = request.EndTime,
                            DurationHours = durationHours,
                            LessonContent = request.LessonContent,
                            Status = AppEnums.SessionStatus.Scheduled.ToString(),
                            CreatedAt = DateTime.UtcNow
                        };
                        await _sessionRepository.AddAsync(recurringSession);
                    }
                }
            }
            else if (pattern == "MONTHLY_NTH_WEEKDAY" || pattern == "MONTHLY_LAST_WEEKDAY" || pattern == "MONTHLY")
            {
                int months = Math.Min(count, 12);
                for (int m = 1; m <= months; m++)
                {
                    var nextDate = startDate.AddMonths(m);
                    var recurringSession = new Session
                    {
                        ClassId = request.ClassId,
                        SessionDate = nextDate,
                        StartTime = request.StartTime,
                        EndTime = request.EndTime,
                        DurationHours = durationHours,
                        LessonContent = request.LessonContent,
                        Status = AppEnums.SessionStatus.Scheduled.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _sessionRepository.AddAsync(recurringSession);
                }
            }
            else if (pattern == "YEARLY")
            {
                for (int y = 1; y <= 2; y++)
                {
                    var nextDate = startDate.AddYears(y);
                    var recurringSession = new Session
                    {
                        ClassId = request.ClassId,
                        SessionDate = nextDate,
                        StartTime = request.StartTime,
                        EndTime = request.EndTime,
                        DurationHours = durationHours,
                        LessonContent = request.LessonContent,
                        Status = AppEnums.SessionStatus.Scheduled.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _sessionRepository.AddAsync(recurringSession);
                }
            }
            else // WEEKLY, CUSTOM or default
            {
                int weeks = count > 1 ? count : (request.RecurringWeeks > 1 ? request.RecurringWeeks : 4);
                for (int w = 1; w < weeks; w++)
                {
                    var nextDate = startDate.AddDays(w * 7);
                    var recurringSession = new Session
                    {
                        ClassId = request.ClassId,
                        SessionDate = nextDate,
                        StartTime = request.StartTime,
                        EndTime = request.EndTime,
                        DurationHours = durationHours,
                        LessonContent = request.LessonContent,
                        Status = AppEnums.SessionStatus.Scheduled.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _sessionRepository.AddAsync(recurringSession);
                }
            }
        }

        public async Task<SessionRespondModel> EditSessionsAsync(SessionRequestModel sessionRequestModel)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            // 1. Tìm buổi học cần sửa
            var session = await _sessionRepository.GetByIdAsync(sessionRequestModel.Id);
            if (session == null)
                throw new NotFoundException("Không tìm thấy thông tin ca học.");

            // 2. Kiểm tra quyền sở hữu của giáo viên đối với lớp học hiện tại
            var currentClass = await _classRepository.GetByIdAsync(session.ClassId);
            if (currentClass == null || currentClass.UserId != userId.Value)
                throw new NotFoundException("Không tìm thấy ca học hoặc bạn không có quyền chỉnh sửa.");

            // 3. Nếu có đổi sang lớp học khác, kiểm tra lớp mới có thuộc về giáo viên không
            if (sessionRequestModel.ClassId > 0 && sessionRequestModel.ClassId != session.ClassId)
            {
                var newClass = await _classRepository.GetByIdAsync(sessionRequestModel.ClassId);
                if (newClass == null || newClass.UserId != userId.Value)
                    throw new BadRequestException("Lớp học chuyển tới không hợp lệ hoặc bạn không có quyền truy cập.");

                currentClass = newClass;
                session.ClassId = sessionRequestModel.ClassId;
            }

            // 4. Tính toán thời lượng buổi học (DurationHours) nếu chưa có
            var durationHours = sessionRequestModel.DurationHours;
            if (durationHours <= 0 && sessionRequestModel.EndTime > sessionRequestModel.StartTime)
            {
                durationHours = (decimal)(sessionRequestModel.EndTime - sessionRequestModel.StartTime).TotalHours;
            }

            // 5. Cập nhật thông tin ca học
            session.SessionDate = sessionRequestModel.SessionDate.Date;
            session.StartTime = sessionRequestModel.StartTime;
            session.EndTime = sessionRequestModel.EndTime;
            session.DurationHours = durationHours > 0 ? durationHours : session.DurationHours;
            session.LessonContent = sessionRequestModel.LessonContent;

            if (!string.IsNullOrWhiteSpace(sessionRequestModel.Status))
            {
                session.Status = sessionRequestModel.Status;
            }

            await _sessionRepository.UpdateAsync(session);

            // 6. Nếu có chọn lặp lại trong khi sửa ca học, sinh thêm các ca học trong tương lai
            var pattern = (sessionRequestModel.RecurrencePattern ?? "NONE").ToUpperInvariant();
            var count = sessionRequestModel.RecurringCount > 0 ? sessionRequestModel.RecurringCount : (sessionRequestModel.RecurringWeeks > 0 ? sessionRequestModel.RecurringWeeks : 4);

            if (pattern != "NONE" || (sessionRequestModel.IsRecurring && count > 1))
            {
                await GenerateRecurringSessionsAsync(sessionRequestModel, session.DurationHours, count);
            }

            // 7. Trả về kết quả sau khi cập nhật
            return new SessionRespondModel
            {
                Id = session.Id,
                ClassId = session.ClassId,
                ClassName = currentClass.ClassName,
                SessionDate = session.SessionDate,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                DurationHours = session.DurationHours,
                LessonContent = session.LessonContent,
                Status = session.Status
            };
        }

        public async Task<IEnumerable<SessionRespondModel>> GetMyTeacherSessionsAsync(DateOnly? fromDate, DateOnly? toDate)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            var mySessions = await _sessionRepository.GetMyTeacherSessionsAsync(fromDate, toDate, userId.Value);

            return mySessions.Select(s =>
            {
                var enrollments = s.Class?.ClassEnrollments?.ToList() ?? new List<ClassEnrollment>();
                var firstStudent = enrollments.FirstOrDefault()?.Student;
                var classType = s.Class?.ClassType ?? (enrollments.Count > 1 ? "Group" : "Individual");
                var feePerSession = s.Class?.DefaultFeePerSession ?? 0;
                var logs = s.AttendanceLogs?.ToList() ?? new List<AttendanceLog>();
                var presentCount = logs.Count(l => string.Equals(l.AttendanceStatus, "Present", StringComparison.OrdinalIgnoreCase) || string.Equals(l.AttendanceStatus, "Có mặt", StringComparison.OrdinalIgnoreCase));
                var hwScores = logs.Select(l => l.HomeworkScore).ToList();
                int? avgHw = hwScores.Any() ? (int)Math.Round((double)hwScores.Average()) : null;

                return new SessionRespondModel
                {
                    Id = s.Id,
                    ClassId = s.ClassId,
                    ClassName = s.Class?.ClassName,
                    ClassType = classType,
                    FeePerSession = feePerSession,
                    StudentsCount = enrollments.Count,
                    StudentName = firstStudent?.FullName,
                    SessionDate = s.SessionDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    DurationHours = s.DurationHours,
                    LessonContent = s.LessonContent,
                    Status = s.Status,
                    PresentCount = presentCount,
                    AvgHomeworkScore = avgHw
                };
            });
        }

        public async Task<string> DeleteSessionAsync(int sessionId)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            // 1. Tìm ca học theo sessionId
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                throw new NotFoundException("Không tìm thấy thông tin ca học.");

            // 2. Kiểm tra quyền sở hữu của giáo viên đối với lớp học
            var @class = await _classRepository.GetByIdAsync(session.ClassId);
            if (@class == null || @class.UserId != userId.Value)
                throw new NotFoundException("Không tìm thấy ca học hoặc bạn không có quyền xóa.");

            // 3. Xóa ca học khỏi cơ sở dữ liệu
            await _sessionRepository.DeleteAsync(session);

            return "Xóa ca học thành công.";
        }

        public async Task<SessionAttendanceDetailResponseModel> GetSessionAttendanceAsync(int sessionId)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");
 
            // 1. Lấy thông tin Session cùng với Lớp học, danh sách học sinh ghi danh và dữ liệu điểm danh
            var session = await _sessionRepository.GetSessionWithAttendanceAsync(sessionId);
            if (session == null || session.Class == null || session.Class.UserId != userId.Value)
                throw new NotFoundException("Không tìm thấy ca học hoặc bạn không có quyền truy cập.");

            // 2. Tạo Dictionary các bản ghi điểm danh đã lưu
            var attendanceDict = session.AttendanceLogs
                .ToDictionary(a => a.StudentId, a => a);

            // 3. Lấy danh sách học sinh đang theo học trong lớp
            var enrolledStudents = session.Class.ClassEnrollments
                .Where(ce => ce.Status != AppEnums.StudentStatus.Deleted.ToString() 
                          && ce.Student != null 
                          && ce.Student.Status != AppEnums.StudentStatus.Deleted.ToString())
                .Select(ce => ce.Student!)
                .ToList();

            var allStudentsDict = new Dictionary<int, Student>();
            foreach (var student in enrolledStudents)
            {
                allStudentsDict[student.Id] = student;
            }
            foreach (var log in session.AttendanceLogs)
            {
                if (log.Student != null && !allStudentsDict.ContainsKey(log.StudentId))
                {
                    allStudentsDict[log.StudentId] = log.Student;
                }
            }

            // 4. Map danh sách học sinh và trạng thái điểm danh
            var studentAttendanceList = new List<StudentAttendanceItemModel>();
            foreach (var student in allStudentsDict.Values.OrderBy(s => s.FullName))
            {
                if (attendanceDict.TryGetValue(student.Id, out var log))
                {
                    studentAttendanceList.Add(new StudentAttendanceItemModel
                    {
                        StudentId = student.Id,
                        StudentName = student.FullName,
                        ParentPhone = student.ParentPhone,
                        AttendanceLogId = log.Id,
                        AttendanceStatus = log.AttendanceStatus,
                        HomeworkScore = log.HomeworkScore,
                        Attitude = log.Attitude,
                        IndividualNote = log.IndividualNote
                    });
                }
                else
                {
                    studentAttendanceList.Add(new StudentAttendanceItemModel
                    {
                        StudentId = student.Id,
                        StudentName = student.FullName,
                        ParentPhone = student.ParentPhone,
                        AttendanceLogId = null,
                        AttendanceStatus = string.Empty,
                        HomeworkScore = 0,
                        Attitude = string.Empty,
                        IndividualNote = null
                    });
                }
            }

            // 5. Thống kê chuyên cần
            var presentCount = studentAttendanceList.Count(s => s.AttendanceStatus == AppEnums.AttendanceStatus.Present.ToString());
            var absentCount = studentAttendanceList.Count(s => 
                s.AttendanceStatus == AppEnums.AttendanceStatus.Absent.ToString() || 
                s.AttendanceStatus == AppEnums.AttendanceStatus.Excused.ToString());

            return new SessionAttendanceDetailResponseModel
            {
                SessionId = session.Id,
                ClassId = session.ClassId,
                ClassName = session.Class.ClassName,
                ClassType = session.Class.ClassType,
                SessionDate = session.SessionDate,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                DurationHours = session.DurationHours,
                LessonContent = session.LessonContent,
                Status = session.Status,
                TotalStudents = studentAttendanceList.Count,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                Students = studentAttendanceList
            };
        }

        public async Task<string> SaveBulkAttendanceAsync(int sessionId, BulkAttendanceRequestModel request)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            // 1. Lấy thông tin ca học kèm danh sách AttendanceLogs hiện có
            var session = await _sessionRepository.GetSessionWithAttendanceAsync(sessionId);
            if (session == null || session.Class == null || session.Class.UserId != userId.Value)
                throw new NotFoundException("Không tìm thấy ca học hoặc bạn không có quyền điểm danh cho ca này.");

            // 2. Duyệt qua danh sách học sinh được gửi lên và thực hiện Upsert (Cập nhật hoặc Thêm mới)
            foreach (var item in request.Attendances)
            {
                var existingLog = session.AttendanceLogs.FirstOrDefault(a => a.StudentId == item.StudentId);

                if (existingLog != null)
                {
                    // Cập nhật bản ghi điểm danh đã tồn tại
                    existingLog.AttendanceStatus = item.AttendanceStatus;
                    existingLog.HomeworkScore = item.HomeworkScore;
                    existingLog.Attitude = item.Attitude;
                    existingLog.IndividualNote = item.IndividualNote;
                }
                else
                {
                    // Thêm bản ghi điểm danh mới cho học sinh này
                    session.AttendanceLogs.Add(new AttendanceLog
                    {
                        SessionId = session.Id,
                        StudentId = item.StudentId,
                        AttendanceStatus = item.AttendanceStatus,
                        HomeworkScore = item.HomeworkScore,
                        Attitude = item.Attitude,
                        IndividualNote = item.IndividualNote,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // 3. Tự động chuyển ca học sang trạng thái Completed nếu trước đó đang Scheduled
            if (session.Status == AppEnums.SessionStatus.Scheduled.ToString())
            {
                session.Status = AppEnums.SessionStatus.Completed.ToString();
            }

            // 4. Lưu toàn bộ thay đổi xuống Database
            await _sessionRepository.UpdateAsync(session);

            return "Lưu điểm danh cả lớp thành công.";
        }

        public async Task<ClassSessionAttendanceResponseModel> SaveClassSessionAttendanceAsync(ClassSessionAttendanceRequestModel request)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            // 1. Tìm lớp học theo ClassId hoặc ClassName
            Class? @class = null;
            if (request.ClassId.HasValue && request.ClassId.Value > 0)
            {
                @class = await _classRepository.GetByIdAsync(request.ClassId.Value);
            }
            if (@class == null && !string.IsNullOrWhiteSpace(request.ClassName))
            {
                @class = await _classRepository.FirstOrDefaultAsync(c =>
                    c.UserId == userId.Value &&
                    c.ClassName == request.ClassName);
            }

            if (@class == null || @class.UserId != userId.Value)
                throw new NotFoundException("Không tìm thấy lớp học hoặc bạn không có quyền điểm danh cho lớp này.");

            var sessionDate = request.SessionDate.Date;

            // 2. Tìm ca học đã có trong ngày cho lớp này, hoặc tạo mới
            var existingSessions = await _sessionRepository.FindAsync(s => s.ClassId == @class.Id && s.SessionDate.Date == sessionDate);
            var session = existingSessions.FirstOrDefault();

            var startTime = request.StartTime ?? new TimeSpan(18, 0, 0);
            var endTime = request.EndTime ?? new TimeSpan(20, 0, 0);
            var duration = (decimal)(endTime - startTime).TotalHours;
            if (duration <= 0) duration = 2;

            if (session == null)
            {
                session = new Session
                {
                    ClassId = @class.Id,
                    SessionDate = sessionDate,
                    StartTime = startTime,
                    EndTime = endTime,
                    DurationHours = duration,
                    LessonContent = request.LessonContent,
                    Status = AppEnums.SessionStatus.Completed.ToString(),
                    CreatedAt = DateTime.UtcNow
                };
                session = await _sessionRepository.AddAsync(session);
            }
            else
            {
                session.LessonContent = request.LessonContent;
                session.Status = AppEnums.SessionStatus.Completed.ToString();
                session.StartTime = startTime;
                session.EndTime = endTime;
                session.DurationHours = duration;
            }

            // Lấy lại session kèm AttendanceLogs để thực hiện upsert
            session = await _sessionRepository.GetSessionWithAttendanceAsync(session.Id);
            if (session == null)
            {
                throw new NotFoundException("Lỗi không thể tải ca học sau khi tạo.");
            }

            // 3. Upsert từng bản ghi điểm danh
            int presentCount = 0;
            int excusedCount = 0;
            int absentCount = 0;

            foreach (var item in request.Attendances)
            {
                var normStatus = item.AttendanceStatus?.Trim();
                if (string.Equals(normStatus, "present", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(normStatus, "Có mặt", StringComparison.OrdinalIgnoreCase))
                {
                    normStatus = AppEnums.AttendanceStatus.Present.ToString();
                    presentCount++;
                }
                else if (string.Equals(normStatus, "excused", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(normStatus, "Nghỉ phép", StringComparison.OrdinalIgnoreCase))
                {
                    normStatus = AppEnums.AttendanceStatus.Excused.ToString();
                    excusedCount++;
                }
                else
                {
                    normStatus = AppEnums.AttendanceStatus.Absent.ToString();
                    absentCount++;
                }

                var existingLog = session.AttendanceLogs.FirstOrDefault(a => a.StudentId == item.StudentId);
                if (existingLog != null)
                {
                    existingLog.AttendanceStatus = normStatus;
                    existingLog.HomeworkScore = item.HomeworkScore;
                    existingLog.Attitude = item.Attitude;
                    existingLog.IndividualNote = item.IndividualNote;
                }
                else
                {
                    session.AttendanceLogs.Add(new AttendanceLog
                    {
                        SessionId = session.Id,
                        StudentId = item.StudentId,
                        AttendanceStatus = normStatus,
                        HomeworkScore = item.HomeworkScore,
                        Attitude = item.Attitude,
                        IndividualNote = item.IndividualNote,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _sessionRepository.UpdateAsync(session);

            var totalFee = presentCount * @class.DefaultFeePerSession;

            return new ClassSessionAttendanceResponseModel
            {
                SessionId = session.Id,
                ClassId = @class.Id,
                ClassName = @class.ClassName,
                SessionDate = session.SessionDate,
                TotalStudents = request.Attendances.Count,
                PresentCount = presentCount,
                ExcusedCount = excusedCount,
                AbsentCount = absentCount,
                FeePerSession = @class.DefaultFeePerSession,
                TotalFeeCalculated = totalFee,
                Message = $"Đã lưu điểm danh ca dạy và ghi nhận {presentCount} học sinh có mặt thành công!"
            };
        }

        public async Task<ClassSessionAttendanceDetailModel> GetClassSessionAttendanceAsync(int? classId, string? className, DateTime? sessionDate)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            Class? @class = null;
            if (classId.HasValue && classId.Value > 0)
            {
                @class = await _classRepository.GetByIdAsync(classId.Value);
            }
            if (@class == null && !string.IsNullOrWhiteSpace(className))
            {
                @class = await _classRepository.FirstOrDefaultAsync(c =>
                    c.UserId == userId.Value &&
                    c.ClassName == className);
            }

            var targetDate = (sessionDate ?? DateTime.Today).Date;

            if (@class == null || @class.UserId != userId.Value)
            {
                return new ClassSessionAttendanceDetailModel
                {
                    ClassId = classId,
                    ClassName = className,
                    SessionDate = targetDate,
                    HasRecorded = false,
                    LessonContent = string.Empty,
                    Attendances = new List<StudentAttendanceUpdateItemModel>()
                };
            }

            var existingSessions = await _sessionRepository.FindAsync(s => s.ClassId == @class.Id && s.SessionDate.Date == targetDate);
            var session = existingSessions.FirstOrDefault();

            if (session == null)
            {
                return new ClassSessionAttendanceDetailModel
                {
                    ClassId = @class.Id,
                    ClassName = @class.ClassName,
                    SessionDate = targetDate,
                    HasRecorded = false,
                    LessonContent = string.Empty,
                    Attendances = new List<StudentAttendanceUpdateItemModel>()
                };
            }

            session = await _sessionRepository.GetSessionWithAttendanceAsync(session.Id);
            var logs = session?.AttendanceLogs?.ToList() ?? new List<AttendanceLog>();

            return new ClassSessionAttendanceDetailModel
            {
                SessionId = session?.Id,
                ClassId = @class.Id,
                ClassName = @class.ClassName,
                SessionDate = session?.SessionDate ?? targetDate,
                LessonContent = session?.LessonContent ?? string.Empty,
                HasRecorded = logs.Any(),
                Attendances = logs.Select(l => new StudentAttendanceUpdateItemModel
                {
                    StudentId = l.StudentId,
                    AttendanceStatus = l.AttendanceStatus,
                    HomeworkScore = l.HomeworkScore,
                    Attitude = l.Attitude ?? string.Empty,
                    IndividualNote = l.IndividualNote
                }).ToList()
            };
        }

    }
}
