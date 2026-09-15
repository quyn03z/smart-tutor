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
using static SmartTutor.BusinessLogic.Models.StudentModels;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IClaimService _claimService;

        public StudentService(IStudentRepository studentRepository, IClaimService claimService)
        {
            _studentRepository = studentRepository;
            _claimService = claimService;
        }

        public async Task<StudentsResponseModel> CreateStudentAsync(RequestStudentModel createStudentModel)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập hoặc token không hợp lệ.");

            var student = new Student
            {
                UserId = userId.Value,
                FullName = createStudentModel.FullName,
                GradeLevel = createStudentModel.GradeLevel,
                ParentName = createStudentModel.ParentName,
                ParentPhone = createStudentModel.ParentPhone,
                CreditBalance = 0,
                Status = AppEnums.StudentStatus.Active.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            ClassEnrollment enrollment;

            // Trường hợp A: Chọn lớp có sẵn (ClassId > 0)
            if (createStudentModel.ClassId.HasValue && createStudentModel.ClassId.Value > 0)
            {
                enrollment = new ClassEnrollment
                {
                    ClassId = createStudentModel.ClassId.Value,
                    CustomFee = createStudentModel.CustomFee ?? createStudentModel.FeePerSession,
                    JoinedDate = DateTime.UtcNow,
                    Status = AppEnums.EnrollmentStatus.Active.ToString()
                };
                student.ClassEnrollments.Add(enrollment);
            }

            // Trường hợp B: Dạy 1-1 (Nhập trực tiếp ClassName và FeePerSession)
            else if (!string.IsNullOrWhiteSpace(createStudentModel.ClassName))
            {
                var newClass = new Class
                {
                    UserId = userId.Value,
                    ClassName = createStudentModel.ClassName,
                    ClassType = createStudentModel.ClassType ?? AppEnums.ClassType.Individual.ToString(),
                    DefaultFeePerSession = createStudentModel.FeePerSession ?? 0,
                    CreatedAt = DateTime.UtcNow
                };
                enrollment = new ClassEnrollment
                {
                    Class = newClass,
                    CustomFee = createStudentModel.FeePerSession,
                    JoinedDate = DateTime.UtcNow,
                    Status = AppEnums.StudentStatus.Active.ToString()
                };
                student.ClassEnrollments.Add(enrollment);
            }

            var createdStudent = await _studentRepository.AddAsync(student);

            var currentEnrollment = createdStudent.ClassEnrollments.FirstOrDefault();

            return new StudentsResponseModel
            {
                Id = createdStudent.Status,
                FullName = createdStudent.FullName,
                GradeLevel = createdStudent.GradeLevel,
                ParentName = createdStudent.ParentName,
                ParentPhone = createdStudent.ParentPhone,
                ClassName = currentEnrollment?.Class?.ClassName,
                ClassType = currentEnrollment?.Class?.ClassType,
                FeePerSession = currentEnrollment?.CustomFee
            };


        }

        public async Task<StudentsResponseModel> EditStudentAsync(RequestStudentModel requestStudentModel)
        {
            var userId = _claimService.GetUserId();
            if (userId == null)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            var student = await _studentRepository.GetStudentByStudentId(requestStudentModel.Id);

            if (student == null || student.UserId != userId.Value)
                throw new NotFoundException("Không tìm thấy học sinh hoặc bạn không có quyền chỉnh sửa.");

            student.FullName = requestStudentModel.FullName;
            student.ParentPhone = requestStudentModel.ParentPhone;
            student.ParentName= requestStudentModel.ParentName;
            student.GradeLevel = requestStudentModel.GradeLevel;
            
            var enrollment = student.ClassEnrollments.FirstOrDefault();

            if(enrollment?.Class != null){
                if (!string.IsNullOrWhiteSpace(requestStudentModel.ClassName))
                    enrollment.Class.ClassName = requestStudentModel.ClassName;
                if (!string.IsNullOrWhiteSpace(requestStudentModel.ClassType))
                    enrollment.Class.ClassType = requestStudentModel.ClassType;
                if (requestStudentModel.FeePerSession.HasValue)
                    enrollment.Class.DefaultFeePerSession = requestStudentModel.FeePerSession.Value;    
            }

            // Case 2: Chuyển sang lớp có sẵn từ danh sách
            if(requestStudentModel.ClassId.HasValue && requestStudentModel.ClassId.Value > 0
             && enrollment?.ClassId != requestStudentModel.ClassId.Value){
                    enrollment?.ClassId = requestStudentModel.ClassId.Value;
            }

            await _studentRepository.UpdateAsync(student);
            return new StudentsResponseModel
            {
                Id = student.Status,
                FullName = student.FullName,
                GradeLevel = student.GradeLevel,
                ParentName = student.ParentName,
                ParentPhone = student.ParentPhone,
                ClassName = enrollment?.Class?.ClassName,
                ClassType = enrollment?.Class?.ClassType,
                FeePerSession = enrollment?.CustomFee
            };


        }

        public async Task<IEnumerable<StudentsResponseModel>> GetStudentsByCurrentUserAsync()
        {
            var userId = _claimService.GetUserId();

            var allsStudents = await _studentRepository.GetAllStudentByUserId(userId.Value);

            return allsStudents.Select(s => {
                var enrollment = s.ClassEnrollments.FirstOrDefault();
                return new StudentsResponseModel
                    {
                        FullName = s.FullName,
                        ParentName = s.ParentName,
                        ParentPhone = s.ParentPhone,
                        GradeLevel = s.GradeLevel,
                        ClassName = enrollment.Class.ClassName,
                        ClassType = enrollment.Class.ClassType,
                        FeePerSession = enrollment.CustomFee,
                };
            });
            
        }

        public async Task<string> DeleteStudentAsync(int studentId)
        {
            var userId = _claimService.GetUserId();
            if (userId == null)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            var student = await _studentRepository.GetStudentByStudentId(studentId);
            if (student == null || student.UserId != userId.Value)
                throw new NotFoundException("Không tìm thấy học sinh hoặc bạn không có quyền xóa.");
                
            student.Status = AppEnums.StudentStatus.Deleted.ToString();
            foreach (var enrollment in student.ClassEnrollments)
            {
                enrollment.Status = AppEnums.StudentStatus.Deleted.ToString();
            }
            await _studentRepository.UpdateAsync(student);
            return "Xóa học sinh thành công.";
        }

        public async Task<StudentDetailResponseModel> GetStudentDetailAsync(int studentId)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            var student = await _studentRepository.GetStudentDetailAsync(studentId);
            if (student == null || student.UserId != userId.Value)
                throw new NotFoundException("Không tìm thấy thông tin học sinh hoặc bạn không có quyền truy cập.");

            return new StudentDetailResponseModel
            {
                Id = student.Id,
                FullName = student.FullName,
                GradeLevel = student.GradeLevel,
                ParentName = student.ParentName,
                ParentPhone = student.ParentPhone,
                CreditBalance = student.CreditBalance,
                Status = student.Status,
                CreatedAt = student.CreatedAt,

                // Map danh sách lớp học
                Classes = student.ClassEnrollments.Select(ce => new StudentClassEnrollmentModel
                {
                    ClassId = ce.ClassId,
                    ClassName = ce.Class?.ClassName ?? string.Empty,
                    ClassType = ce.Class?.ClassType ?? string.Empty,
                    FeePerSession = ce.CustomFee ?? ce.Class?.DefaultFeePerSession,
                    Status = ce.Status,
                    JoinedDate = ce.JoinedDate
                }).ToList(),

                // Map lịch sử học tập từng buổi (sắp xếp buổi mới nhất lên đầu)
                AttendanceHistory = student.AttendanceLogs
                    .OrderByDescending(al => al.Session?.SessionDate)
                    .Select(al => new StudentAttendanceHistoryModel
                    {
                        SessionId = al.SessionId,
                        SessionDate = al.Session?.SessionDate ?? DateTime.MinValue,
                        LessonContent = al.Session?.LessonContent,
                        AttendanceStatus = al.AttendanceStatus,
                        HomeworkScore = al.HomeworkScore,
                        Attitude = al.Attitude,
                        IndividualNote = al.IndividualNote
                    }).ToList()
            };
        }

        public async Task<StudentCreditHistoryResponseModel> GetStudentCreditHistoryAsync(int studentId)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            var student = await _studentRepository.GetStudentWithCreditHistoryAsync(studentId);
            if (student == null || student.UserId != userId.Value)
                throw new NotFoundException("Không tìm thấy thông tin học sinh hoặc bạn không có quyền truy cập.");

            var history = new List<CreditHistoryItemModel>();

            // 1. Giao dịch nạp tiền trực tiếp vào số dư (PaymentTransactions không qua báo cáo tháng)
            if (student.PaymentTransactions != null)
            {
                foreach (var tx in student.PaymentTransactions.Where(t => t.ReportId == null))
                {
                    history.Add(new CreditHistoryItemModel
                    {
                        Id = $"tx-{tx.Id}",
                        TransactionType = "PLUS",
                        Action = "TOPUP",
                        Title = $"Nạp tiền trả trước ({tx.Gateway})",
                        Description = $"Nạp tiền trả trước vào tài khoản qua cổng {tx.Gateway}",
                        Amount = tx.AmountIn,
                        BalanceChange = tx.AmountIn,
                        TransactionDate = tx.TransactionTime != default ? tx.TransactionTime : tx.CreatedAt,
                        ReferenceCode = tx.TransactionId,
                        Gateway = tx.Gateway
                    });
                }
            }

            // 2. Lịch sử khấu trừ học phí & cộng tiền thừa từ Báo cáo tháng (MonthlyReports)
            if (student.MonthlyReports != null)
            {
                foreach (var report in student.MonthlyReports)
                {
                    var className = report.Class?.ClassName;

                    // Khấu trừ số dư trả trước vào học phí tháng (CreditDeducted > 0)
                    if (report.CreditDeducted > 0)
                    {
                        history.Add(new CreditHistoryItemModel
                        {
                            Id = $"deduct-report-{report.Id}",
                            TransactionType = "MINUS",
                            Action = "DEDUCTION",
                            Title = $"Khấu trừ học phí tháng {report.ReportMonth}",
                            Description = string.IsNullOrEmpty(className)
                                ? $"Khấu trừ số dư trả trước vào học phí tháng {report.ReportMonth}"
                                : $"Khấu trừ số dư trả trước vào học phí tháng {report.ReportMonth} (Lớp {className})",
                            Amount = report.CreditDeducted,
                            BalanceChange = -report.CreditDeducted,
                            TransactionDate = report.CreatedAt,
                            ReferenceCode = report.TransferCode,
                            ReportId = report.Id,
                            ReportMonth = report.ReportMonth,
                            ClassName = className
                        });
                    }

                    // Cộng số dư khi phụ huynh đóng thừa tiền học phí tháng (OverpaidAmount > 0)
                    if (report.OverpaidAmount > 0)
                    {
                        var latestTx = report.PaymentTransactions?.OrderByDescending(p => p.TransactionTime).FirstOrDefault();
                        history.Add(new CreditHistoryItemModel
                        {
                            Id = $"overpaid-report-{report.Id}",
                            TransactionType = "PLUS",
                            Action = "OVERPAID",
                            Title = $"Cộng số dư thừa tháng {report.ReportMonth}",
                            Description = string.IsNullOrEmpty(className)
                                ? $"Cộng tiền thanh toán thừa sau quyết toán học phí tháng {report.ReportMonth} vào số dư"
                                : $"Cộng tiền thanh toán thừa sau quyết toán học phí tháng {report.ReportMonth} (Lớp {className}) vào số dư",
                            Amount = report.OverpaidAmount,
                            BalanceChange = report.OverpaidAmount,
                            TransactionDate = latestTx?.TransactionTime ?? report.CreatedAt,
                            ReferenceCode = latestTx?.TransactionId ?? report.TransferCode,
                            Gateway = latestTx?.Gateway,
                            ReportId = report.Id,
                            ReportMonth = report.ReportMonth,
                            ClassName = className
                        });
                    }
                }
            }

            return new StudentCreditHistoryResponseModel
            {
                StudentId = student.Id,
                FullName = student.FullName,
                CurrentCreditBalance = student.CreditBalance,
                History = history.OrderByDescending(h => h.TransactionDate).ToList()
            };
        }

    }
}
