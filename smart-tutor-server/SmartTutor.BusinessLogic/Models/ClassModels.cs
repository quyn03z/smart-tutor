using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartTutor.BusinessLogic.Models
{
    public class ClassModels
    {

        public class ClassResponseModel
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string ClassName { get; set; } = string.Empty;
            public string ClassType { get; set; } = string.Empty;
            public decimal DefaultFeePerSession { get; set; }
            public string? SchedulePattern { get; set; }
        }

        public class ClassRequestModel 
        {
            public int Id { get; set; }
            public int UserId { get; set; }

            [Required(ErrorMessage = "Tên lớp là bắt buộc.")]
            [StringLength(100, ErrorMessage = "Tên lớp không được vượt quá 100 ký tự.")]
            public string ClassName { get; set; } = string.Empty;
            public string ClassType { get; set; } = string.Empty;
            public decimal DefaultFeePerSession { get; set; }
            public string? SchedulePattern { get; set; }

        }

    }
}
