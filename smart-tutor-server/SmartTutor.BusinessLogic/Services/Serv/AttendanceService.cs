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
        private readonly IClaimService _claimService;

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            IClaimService claimService)
        {
            _attendanceRepository = attendanceRepository;
            _claimService = claimService;
        }

        public async Task<UpdateAttendanceResponseDto> UpdateAttendanceAsync(UpdateAttendanceRequestDto updateAttendanceRequestDto)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            // 1. Tìm bản ghi điểm danh theo AttendanceId kèm thông tin Student và Class
            var attendance = await _attendanceRepository.Query()
                .Include(a => a.Student)
                .Include(a => a.Session)
                    .ThenInclude(s => s!.Class)
                .FirstOrDefaultAsync(a => a.Id == updateAttendanceRequestDto.AttendanceId);

            if (attendance == null)
                throw new NotFoundException("Không tìm thấy bản ghi điểm danh.");

            // 2. Kiểm tra quyền sở hữu của giáo viên đối với lớp học
            var classUserId = attendance.Session?.Class?.UserId;
            if (!classUserId.HasValue || classUserId.Value != userId.Value)
                throw new NotFoundException("Không tìm thấy bản ghi điểm danh hoặc bạn không có quyền chỉnh sửa.");

            // 3. Cập nhật thông tin điểm danh
            attendance.AttendanceStatus = updateAttendanceRequestDto.AttendanceStatus;
            attendance.HomeworkScore = updateAttendanceRequestDto.HomeworkScore;
            attendance.Attitude = updateAttendanceRequestDto.Attitude;
            attendance.IndividualNote = updateAttendanceRequestDto.IndividualNote;

            await _attendanceRepository.UpdateAsync(attendance);

            // 4. Trả về kết quả
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
