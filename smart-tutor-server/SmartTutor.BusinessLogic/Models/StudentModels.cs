using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartTutor.BusinessLogic.Models
{
    public class StudentModels
    {

        public class StudentsResponseModel
        {
            public string Id { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string? ClassName { get; set; }
            public string? ClassType { get; set; }
            public string? GradeLevel { get; set; }
            public string? ParentName { get; set; }
            public string? ParentPhone { get; set; }
            public decimal? FeePerSession { get; set; }

        }


        public class RequestStudentModel
        {
            public int Id { get; set; }
            [Required(ErrorMessage = "Họ tên học sinh không được để trống")]
            [StringLength(100)]
            public string FullName { get; set; }
            public string? ClassName { get; set; }
            public string? ClassType { get; set; }
            public string? GradeLevel { get; set; }
            public string? ParentName { get; set; }
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            public string? ParentPhone { get; set; }
            public decimal? FeePerSession { get; set; }
            public int? ClassId { get; set; }
            public decimal? CustomFee { get; set; }

        }

        // Chi tiết học sinh và lịch sử học tập
        public class StudentDetailResponseModel
        {
            public int Id { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string? GradeLevel { get; set; }
            public string? ParentName { get; set; }
            public string? ParentPhone { get; set; }
            public decimal CreditBalance { get; set; } // Số dư tài khoản học phí
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }

            // Thông tin các lớp đang theo học
            public List<StudentClassEnrollmentModel> Classes { get; set; } = new();

            // Lịch sử các buổi học & điểm danh
            public List<StudentAttendanceHistoryModel> AttendanceHistory { get; set; } = new();
        }

        public class StudentClassEnrollmentModel
        {
            public int ClassId { get; set; }
            public string ClassName { get; set; } = string.Empty;
            public string ClassType { get; set; } = string.Empty;
            public decimal? FeePerSession { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime JoinedDate { get; set; }
        }

        public class StudentAttendanceHistoryModel
        {
            public int SessionId { get; set; }
            public DateTime SessionDate { get; set; }     // Ngày học
            public string? LessonContent { get; set; }    // Nội dung bài dạy
            public string AttendanceStatus { get; set; } = string.Empty; // Present, Absent, Late, Excused
            public int HomeworkScore { get; set; }        // Điểm bài tập về nhà
            public string Attitude { get; set; } = string.Empty; // Thái độ học tập
            public string? IndividualNote { get; set; }   // Nhận xét riêng của giáo viên
        }

        // Lịch sử cộng/trừ số dư trả trước của học sinh
        public class StudentCreditHistoryResponseModel
        {
            public int StudentId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public decimal CurrentCreditBalance { get; set; }
            public List<CreditHistoryItemModel> History { get; set; } = new();
        }

        public class CreditHistoryItemModel
        {
            public string Id { get; set; } = string.Empty;
            public string TransactionType { get; set; } = string.Empty; // PLUS (Cộng) | MINUS (Trừ)
            public string Action { get; set; } = string.Empty;          // OVERPAID | DEDUCTION | TOPUP
            public string Title { get; set; } = string.Empty;           // Tiêu đề ngắn gọn
            public string? Description { get; set; }                   // Mô tả chi tiết biến động số dư
            public decimal Amount { get; set; }                        // Số tiền giao dịch (dương)
            public decimal BalanceChange { get; set; }                 // Biến động số dư (+Amount hoặc -Amount)
            public DateTime TransactionDate { get; set; }              // Thời gian phát sinh giao dịch / khấu trừ
            public string? ReferenceCode { get; set; }                 // Mã giao dịch / mã chuyển khoản
            public string? Gateway { get; set; }                       // Cổng thanh toán (VietQR, Cash, Momo...)
            public int? ReportId { get; set; }                         // ID báo cáo tháng liên quan (nếu có)
            public string? ReportMonth { get; set; }                   // Tháng báo cáo (YYYY-MM)
            public string? ClassName { get; set; }                     // Tên lớp học liên quan
        }

    }
}
