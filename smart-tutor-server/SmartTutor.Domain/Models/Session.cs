using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTutor.Domain.Models
{
    public class Session
    {
        public int Id { get; set; }

        public int ClassId { get; set; }

        [Column(TypeName = "date")]
        public DateTime SessionDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal DurationHours { get; set; }

        public string? LessonContent { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Class? Class { get; set; }

        public virtual ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();
    }
}
