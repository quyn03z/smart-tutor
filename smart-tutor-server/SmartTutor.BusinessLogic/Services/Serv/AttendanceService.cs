using Microsoft.EntityFrameworkCore;
using SmartTutor.BusinessLogic.Exceptions;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Claims;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static SmartTutor.BusinessLogic.Models.AttendanceModels;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IClassRepository _classRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IClaimService _claimService;

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            ISessionRepository sessionRepository,
            IClassRepository classRepository,
            IStudentRepository studentRepository,
            IClaimService claimService)
        {
            _attendanceRepository = attendanceRepository;
            _sessionRepository = sessionRepository;
            _classRepository = classRepository;
            _studentRepository = studentRepository;
            _claimService = claimService;
        }

        public async Task<UpdateAttendanceResponseDto> UpdateAttendanceAsync(UpdateAttendanceRequestDto updateAttendanceRequestDto)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            AttendanceLog? attendance = null;

            // 1. Tìm bản ghi điểm danh theo AttendanceId (nếu có)
            if (updateAttendanceRequestDto.AttendanceId > 0)
            {
                attendance = await _attendanceRepository.Query()
                    .Include(a => a.Student)
                    .Include(a => a.Session)
                        .ThenInclude(s => s!.Class)
                    .FirstOrDefaultAsync(a => a.Id == updateAttendanceRequestDto.AttendanceId);
            }

            // 2. Nếu không tìm thấy theo AttendanceId, tìm theo cặp (SessionId, StudentId)
            if (attendance == null && updateAttendanceRequestDto.SessionId > 0 && updateAttendanceRequestDto.StudentId > 0)
            {
                attendance = await _attendanceRepository.Query()
                    .Include(a => a.Student)
                    .Include(a => a.Session)
                        .ThenInclude(s => s!.Class)
                    .FirstOrDefaultAsync(a => a.SessionId == updateAttendanceRequestDto.SessionId 
                                           && a.StudentId == updateAttendanceRequestDto.StudentId);
            }

            // 3. Nếu chưa từng điểm danh cho học sinh trong ca này -> Tạo mới (Insert)
            if (attendance == null)
            {
                var session = await _sessionRepository.GetByIdAsync(updateAttendanceRequestDto.SessionId);
                if (session == null)
                    throw new NotFoundException("Không tìm thấy ca học.");

                var @class = await _classRepository.GetByIdAsync(session.ClassId);
                if (@class == null || @class.UserId != userId.Value)
                    throw new NotFoundException("Không tìm thấy ca học hoặc bạn không có quyền chỉnh sửa.");

                var student = await _studentRepository.GetByIdAsync(updateAttendanceRequestDto.StudentId);
                if (student == null)
                    throw new NotFoundException("Không tìm thấy thông tin học sinh.");

                attendance = new AttendanceLog
                {
                    SessionId = updateAttendanceRequestDto.SessionId,
                    StudentId = updateAttendanceRequestDto.StudentId,
                    AttendanceStatus = updateAttendanceRequestDto.AttendanceStatus,
                    HomeworkScore = updateAttendanceRequestDto.HomeworkScore,
                    Attitude = updateAttendanceRequestDto.Attitude,
                    IndividualNote = updateAttendanceRequestDto.IndividualNote,
                    CreatedAt = DateTime.UtcNow
                };

                await _attendanceRepository.AddAsync(attendance);
                attendance.Student = student;
            }
            else
            {
                // 4. Nếu đã có bản ghi -> Kiểm tra quyền và Cập nhật (Update)
                var classUserId = attendance.Session?.Class?.UserId;
                if (!classUserId.HasValue)
                {
                    var session = await _sessionRepository.GetByIdAsync(attendance.SessionId);
                    var @class = session != null ? await _classRepository.GetByIdAsync(session.ClassId) : null;
                    classUserId = @class?.UserId;
                }

                if (classUserId != userId.Value)
                    throw new NotFoundException("Không tìm thấy bản ghi điểm danh hoặc bạn không có quyền chỉnh sửa.");

                attendance.AttendanceStatus = updateAttendanceRequestDto.AttendanceStatus;
                attendance.HomeworkScore = updateAttendanceRequestDto.HomeworkScore;
                attendance.Attitude = updateAttendanceRequestDto.Attitude;
                attendance.IndividualNote = updateAttendanceRequestDto.IndividualNote;

                await _attendanceRepository.UpdateAsync(attendance);
            }

            // 5. Trả về kết quả
            return new UpdateAttendanceResponseDto
            {
                AttendanceId = attendance.Id,
                SessionId = attendance.SessionId,
                StudentId = attendance.StudentId,
                StudentName = attendance.Student?.FullName ?? string.Empty,
                AttendanceStatus = attendance.AttendanceStatus,
                HomeworkScore = attendance.HomeworkScore,
                Attitude = attendance.Attitude,
                IndividualNote = attendance.IndividualNote,
                Message = "Cập nhật điểm danh thành công."
            };
        }
    }
}
