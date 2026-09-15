using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartTutor.BusinessLogic.Models
{
    public class SessionModels
    {
        public class SessionRequestModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Mã lớp học không được để trống.")]
            [Range(1, int.MaxValue, ErrorMessage = "Mã lớp học không hợp lệ.")]
            public int ClassId { get; set; }

            [Required(ErrorMessage = "Ngày học không được để trống.")]
            [DataType(DataType.Date)]
            public DateTime SessionDate { get; set; }

            [Required(ErrorMessage = "Giờ bắt đầu không được để trống.")]
            public TimeSpan StartTime { get; set; }

            [Required(ErrorMessage = "Giờ kết thúc không được để trống.")]
            public TimeSpan EndTime { get; set; }

            [Range(1, 24, ErrorMessage = "Thời lượng buổi học phải từ 1 đến 24 giờ.")]
            public decimal DurationHours { get; set; }

            [StringLength(1000, ErrorMessage = "Nội dung bài dạy không được vượt quá 1000 ký tự.")]
            public string? LessonContent { get; set; }

            [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự.")]
            public string Status { get; set; } = string.Empty;
        }

        public class SessionRespondModel
        {
            public int Id { get; set; }
            public int ClassId { get; set; }
            public DateTime SessionDate { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public decimal DurationHours { get; set; }
            public string? LessonContent { get; set; }
            public string Status { get; set; } = string.Empty;
        }

    }
}
