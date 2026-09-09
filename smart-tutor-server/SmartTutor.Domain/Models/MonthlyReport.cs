using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTutor.Domain.Models
{
    public class MonthlyReport
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int ClassId { get; set; }

        [Required]
        [StringLength(7)]
        public string ReportMonth { get; set; } = string.Empty;

        public int TotalSessions { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TotalHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditDeducted { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OverpaidAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string TransferCode { get; set; } = string.Empty;

        [Required]
        [StringLength(64)]
        public string MagicToken { get; set; } = string.Empty;

        public string? TeacherComment { get; set; }

        public string? Roadmap { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime? ParentViewedAt { get; set; }

        public string? ParentFeedback { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Class? Class { get; set; }

        public virtual Student? Student { get; set; }

        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
}
