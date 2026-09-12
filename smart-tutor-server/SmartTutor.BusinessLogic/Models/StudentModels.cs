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
            public string FullName { get; set; } = string.Empty;
            public string? ClassName { get; set; }
            public string? ClassType { get; set; }
            public string? GradeLevel { get; set; }
            public string? ParentName { get; set; }
            public string? ParentPhone { get; set; }
            public decimal? FeePerSession { get; set; }

        }


    }
}
