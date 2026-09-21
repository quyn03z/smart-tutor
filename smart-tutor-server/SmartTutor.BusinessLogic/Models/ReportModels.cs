using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartTutor.BusinessLogic.Models
{
    public class ReportModels
    {
        // Request DTO sinh báo cáo tháng
        public class GenerateReportRequestDto
        {
            [Required(ErrorMessage = "Mã lớp học không được để trống.")]
            [Range(1, int.MaxValue, ErrorMessage = "Mã lớp học không hợp lệ.")]
            public int ClassId { get; set; }

            [Required(ErrorMessage = "Tháng báo cáo không được để trống.")]
            [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])$", ErrorMessage = "Tháng báo cáo phải có định dạng YYYY-MM (Ví dụ: 2026-09).")]
            public string ReportMonth { get; set; } = string.Empty; // e.g. "2026-09"

            // Tùy chọn: Sinh cho 1 học sinh cụ thể (nếu không truyền sẽ sinh cho tất cả HS trong lớp)
            public int? StudentId { get; set; }

            public string? TeacherComment { get; set; }
            public string? Roadmap { get; set; }
        }

        // Response DTO trả về thông tin tóm tắt báo cáo tháng
        public class MonthlyReportResponseDto
        {
            public int Id { get; set; }
            public int StudentId { get; set; }
            public string StudentName { get; set; } = string.Empty;
            public int ClassId { get; set; }
            public string ClassName { get; set; } = string.Empty;
            public string ReportMonth { get; set; } = string.Empty;
            public int TotalSessions { get; set; }
            public decimal TotalHours { get; set; }
            public decimal GrossAmount { get; set; }
            public decimal CreditDeducted { get; set; }
            public decimal FinalAmount { get; set; }
            public decimal AmountPaid { get; set; }
            public decimal OverpaidAmount { get; set; }
            public string TransferCode { get; set; } = string.Empty;
            public string MagicToken { get; set; } = string.Empty;
            public string? TeacherComment { get; set; }
            public string? Roadmap { get; set; }
            public string PaymentStatus { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }

        // DTO chi tiết từng buổi học và điểm danh của học sinh trong tháng
        public class ReportSessionDetailDto
        {
            public int SessionId { get; set; }
            public DateTime SessionDate { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public decimal DurationHours { get; set; }
            public string? LessonContent { get; set; }
            public string AttendanceStatus { get; set; } = string.Empty; // Present, Absent, Excused, Late
            public int HomeworkScore { get; set; }
            public string Attitude { get; set; } = string.Empty;
            public string? IndividualNote { get; set; }
        }

        // Response DTO chi tiết phiếu báo cáo tháng (Live Preview) kèm danh sách các buổi học
        public class MonthlyReportDetailResponseDto : MonthlyReportResponseDto
        {
            public string? ParentName { get; set; }
            public string? ParentPhone { get; set; }
            public string? GradeLevel { get; set; }
            public List<ReportSessionDetailDto> Sessions { get; set; } = new List<ReportSessionDetailDto>();
        }
    }
}
