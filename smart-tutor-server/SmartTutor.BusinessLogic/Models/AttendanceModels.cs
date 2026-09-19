using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartTutor.BusinessLogic.Models
{
    public class AttendanceModels
    {
        public class UpdateAttendanceRequestDto
        {
            public int AttendanceId { get; set; }
            // Giá trị hợp lệ: "Present", "Excused", "Unexcused", "Compensate"
            [Required(ErrorMessage = "Trạng thái điểm danh không được để trống.")]
            [RegularExpression("^(Present|Absent|Excused|Late)$",
                ErrorMessage = "Trạng thái chỉ có thể là: Present, Absent, Excused, hoặc Late.")]
            public string AttendanceStatus { get; set; } = "Present";

            // Tỷ lệ hoàn thành BTVN (0 -> 100%)
            [Range(0, 100, ErrorMessage = "Tỷ lệ BTVN phải từ 0 đến 100.")]
            public int HomeworkScore { get; set; } = 100;

            [MaxLength(100, ErrorMessage = "Thái độ không được vượt quá 100 ký tự.")]
            public string Attitude { get; set; } = "Tốt";

            // Lời nhắn riêng cho phụ huynh (VD: "Hôm nay em làm bài nhanh, tiến bộ")
            public string? IndividualNote { get; set; }
        }

        public class UpdateAttendanceResponseDto
        {
            public int AttendanceId { get; set; }
            public int SessionId { get; set; }
            public int StudentId { get; set; }
            public string StudentName { get; set; } = string.Empty;
            public string AttendanceStatus { get; set; } = string.Empty;
            public int HomeworkScore { get; set; }
            public string Attitude { get; set; } = string.Empty;
            public string? IndividualNote { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }
}
