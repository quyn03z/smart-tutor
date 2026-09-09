using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTutor.Domain.Models
{
    public class AttendanceLog
    {
        public int Id { get; set; }

        public int SessionId { get; set; }

        public int StudentId { get; set; }

        [Required]
        [StringLength(20)]
        public string AttendanceStatus { get; set; } = string.Empty;

        public int HomeworkScore { get; set; }

        [Required]
        [StringLength(100)]
        public string Attitude { get; set; } = string.Empty;

        public string? IndividualNote { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Session? Session { get; set; }

        public virtual Student? Student { get; set; }
    }
}
