using Microsoft.EntityFrameworkCore;
using SmartTutor.BusinessLogic.Exceptions;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Claims;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.Domain.Enums;
using SmartTutor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static SmartTutor.BusinessLogic.Models.ReportModels;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class RepoortService : IRepoortService
    {
        private readonly IMonthlyReportRepository _monthlyReportRepository;
        private readonly IClassRepository _classRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IClaimService _claimService;

        public RepoortService(
            IMonthlyReportRepository monthlyReportRepository,
            IClassRepository _classRepository,
            ISessionRepository sessionRepository,
            IStudentRepository studentRepository,
            IClaimService claimService)
        {
            _monthlyReportRepository = monthlyReportRepository;
            this._classRepository = _classRepository;
            _sessionRepository = sessionRepository;
            _studentRepository = studentRepository;
            _claimService = claimService;
        }

        public async Task<IEnumerable<MonthlyReportResponseDto>> GenerateMonthlyReportsAsync(GenerateReportRequestDto dto)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            // 1. Kiểm tra lớp học và quyền sở hữu của giáo viên
            var classEntity = await _classRepository.Query()
                .Include(c => c.ClassEnrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId && c.UserId == userId.Value);

            if (classEntity == null)
            {
                throw new NotFoundException("Không tìm thấy lớp học hoặc bạn không có quyền truy cập.");
            }

            // 2. Parse thời gian tháng báo cáo
            var parts = dto.ReportMonth.Split('-');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int year) || !int.TryParse(parts[1], out int month))
            {
                throw new BadRequestException("Định dạng tháng báo cáo không hợp lệ. Vui lòng sử dụng định dạng YYYY-MM.");
            }

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // 3. Lấy tất cả các buổi học trong tháng của lớp
            var sessionsInMonth = await _sessionRepository.Query()
                .Include(s => s.AttendanceLogs)
                .Where(s => s.ClassId == dto.ClassId && s.SessionDate >= startDate && s.SessionDate <= endDate)
                .ToListAsync();

            // 4. Lấy danh sách học sinh cần chốt báo cáo
            var enrollments = classEntity.ClassEnrollments
                .Where(e => e.Status == AppEnums.EnrollmentStatus.Active.ToString() && e.Student != null)
                .ToList();

            if (dto.StudentId.HasValue)
            {
                enrollments = enrollments.Where(e => e.StudentId == dto.StudentId.Value).ToList();
                if (!enrollments.Any())
                {
                    throw new NotFoundException("Không tìm thấy học sinh trong lớp học này.");
                }
            }

            if (!enrollments.Any())
            {
                throw new BadRequestException("Lớp học không có học sinh nào đang hoạt động để tạo báo cáo.");
            }

            var responseList = new List<MonthlyReportResponseDto>();

            // 5. Tính toán và sinh báo cáo cho từng học sinh
            foreach (var enrollment in enrollments)
            {
                var student = enrollment.Student!;

                // 5.1. Tính tổng số buổi thực tế có mặt (Present hoặc Late)
                var attendedSessions = sessionsInMonth
                    .Where(s => s.AttendanceLogs.Any(al => al.StudentId == student.Id &&
                                (al.AttendanceStatus == AppEnums.AttendanceStatus.Present.ToString() ||
                                 al.AttendanceStatus == AppEnums.AttendanceStatus.Late.ToString())))
                    .ToList();

                var totalSessions = attendedSessions.Count;
                var totalHours = attendedSessions.Sum(s => s.DurationHours);

                // 5.2. Tính tổng tiền phát sinh (GrossAmount)
                var feePerSession = enrollment.CustomFee ?? classEntity.DefaultFeePerSession;
                var grossAmount = totalSessions * feePerSession;

                // 5.3. Tự động trừ số dư tích lũy (CreditDeducted lấy từ CreditBalance của học sinh)
                var creditDeducted = 0m;
                if (student.CreditBalance > 0 && grossAmount > 0)
                {
                    creditDeducted = Math.Min(grossAmount, student.CreditBalance);
                    student.CreditBalance -= creditDeducted;
                    await _studentRepository.UpdateAsync(student);
                }

                // 5.4. Tính ra FinalAmount cần thanh toán
                var finalAmount = grossAmount - creditDeducted;
                var paymentStatus = finalAmount == 0
                    ? AppEnums.PaymentStatus.Paid.ToString()
                    : AppEnums.PaymentStatus.Pending.ToString();

                // 5.5. Kiểm tra nếu đã có báo cáo tháng cho học sinh trong lớp này
                var existingReport = await _monthlyReportRepository.Query()
                    .FirstOrDefaultAsync(r => r.ClassId == dto.ClassId
                                           && r.StudentId == student.Id
                                           && r.ReportMonth == dto.ReportMonth);

                MonthlyReport report;
                if (existingReport != null)
                {
                    report = existingReport;
                    report.TotalSessions = totalSessions;
                    report.TotalHours = totalHours;
                    report.GrossAmount = grossAmount;
                    report.CreditDeducted = creditDeducted;
                    report.FinalAmount = finalAmount;
                    report.PaymentStatus = paymentStatus; 

                    if (!string.IsNullOrWhiteSpace(dto.TeacherComment))
                        report.TeacherComment = dto.TeacherComment;
                    if (!string.IsNullOrWhiteSpace(dto.Roadmap))
                        report.Roadmap = dto.Roadmap;

                    if (string.IsNullOrWhiteSpace(report.TransferCode))
                        report.TransferCode = await GenerateUniqueTransferCodeAsync(student.Id, month, year);
                    if (string.IsNullOrWhiteSpace(report.MagicToken))
                        report.MagicToken = await GenerateUniqueMagicTokenAsync();

                    await _monthlyReportRepository.UpdateAsync(report);
                }
                else
                {
                    var transferCode = await GenerateUniqueTransferCodeAsync(student.Id, month, year);
                    var magicToken = await GenerateUniqueMagicTokenAsync();

                    report = new MonthlyReport
                    {
                        StudentId = student.Id,
                        ClassId = dto.ClassId,
                        ReportMonth = dto.ReportMonth,
                        TotalSessions = totalSessions,
                        TotalHours = totalHours,
                        GrossAmount = grossAmount,
                        CreditDeducted = creditDeducted,
                        FinalAmount = finalAmount,
                        AmountPaid = 0,
                        OverpaidAmount = 0,
                        TransferCode = transferCode,
                        MagicToken = magicToken,
                        TeacherComment = dto.TeacherComment,
                        Roadmap = dto.Roadmap,
                        PaymentStatus = paymentStatus,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _monthlyReportRepository.AddAsync(report);
                }

                responseList.Add(new MonthlyReportResponseDto
                {
                    Id = report.Id,
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    ClassId = classEntity.Id,
                    ClassName = classEntity.ClassName,
                    ReportMonth = report.ReportMonth,
                    TotalSessions = report.TotalSessions,
                    TotalHours = report.TotalHours,
                    GrossAmount = report.GrossAmount,
                    CreditDeducted = report.CreditDeducted,
                    FinalAmount = report.FinalAmount,
                    AmountPaid = report.AmountPaid,
                    OverpaidAmount = report.OverpaidAmount,
                    TransferCode = report.TransferCode,
                    MagicToken = report.MagicToken,
                    TeacherComment = report.TeacherComment,
                    Roadmap = report.Roadmap,
                    PaymentStatus = report.PaymentStatus,
                    CreatedAt = report.CreatedAt
                });
            }

            return responseList;
        }

        private async Task<string> GenerateUniqueTransferCodeAsync(int studentId, int month, int year)
        {
            string code;
            bool exists;
            do
            {
                var randomSuffix = RandomNumberGenerator.GetInt32(1000, 9999);
                code = $"ST{studentId}T{month:D2}{year % 100}{randomSuffix}";
                exists = await _monthlyReportRepository.ExistsAsync(r => r.TransferCode == code);
            } while (exists);

            return code;
        }

        private async Task<string> GenerateUniqueMagicTokenAsync()
        {
            string token;
            bool exists;
            do
            {
                var bytes = RandomNumberGenerator.GetBytes(32);
                token = Convert.ToHexString(bytes).ToLowerInvariant();
                exists = await _monthlyReportRepository.ExistsAsync(r => r.MagicToken == token);
            } while (exists);

            return token;
        }

        public async Task<IEnumerable<MonthlyReportResponseDto>> GetMonthlyReportsAsync(string? reportMonth, string? paymentStatus, int? classId)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            var query = _monthlyReportRepository.Query()
                .Include(r => r.Student)
                .Include(r => r.Class)
                .Where(r => r.Class != null && r.Class.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(reportMonth))
            {
                query = query.Where(r => r.ReportMonth == reportMonth);
            }

            if (classId.HasValue)
            {
                query = query.Where(r => r.ClassId == classId.Value);
            }

            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                var status = paymentStatus.Trim();
                if (string.Equals(status, "Unpaid", StringComparison.OrdinalIgnoreCase))
                {
                    var pending = AppEnums.PaymentStatus.Pending.ToString();
                    var partiallyPaid = AppEnums.PaymentStatus.PartiallyPaid.ToString();
                    query = query.Where(r => r.PaymentStatus == pending || r.PaymentStatus == partiallyPaid);
                }
                else if (string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase))
                {
                    var paid = AppEnums.PaymentStatus.Paid.ToString();
                    var overpaid = AppEnums.PaymentStatus.Overpaid.ToString();
                    query = query.Where(r => r.PaymentStatus == paid || r.PaymentStatus == overpaid);
                }
                else
                {
                    query = query.Where(r => r.PaymentStatus == status);
                }
            }

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reports.Select(report => new MonthlyReportResponseDto
            {
                Id = report.Id,
                StudentId = report.StudentId,
                StudentName = report.Student?.FullName ?? string.Empty,
                ClassId = report.ClassId,
                ClassName = report.Class?.ClassName ?? string.Empty,
                ReportMonth = report.ReportMonth,
                TotalSessions = report.TotalSessions,
                TotalHours = report.TotalHours,
                GrossAmount = report.GrossAmount,
                CreditDeducted = report.CreditDeducted,
                FinalAmount = report.FinalAmount,
                AmountPaid = report.AmountPaid,
                OverpaidAmount = report.OverpaidAmount,
                TransferCode = report.TransferCode,
                MagicToken = report.MagicToken,
                TeacherComment = report.TeacherComment,
                Roadmap = report.Roadmap,
                PaymentStatus = report.PaymentStatus,
                CreatedAt = report.CreatedAt
            });
        }
    }
}
