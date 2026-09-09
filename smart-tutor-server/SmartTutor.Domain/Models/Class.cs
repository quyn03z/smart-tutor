using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTutor.Domain.Models
{
    public class Class
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ClassType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DefaultFeePerSession { get; set; }

        public string? SchedulePattern { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }

        public virtual ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();

        public virtual ICollection<MonthlyReport> MonthlyReports { get; set; } = new List<MonthlyReport>();

        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
