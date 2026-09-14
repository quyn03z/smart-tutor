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


    }
}
