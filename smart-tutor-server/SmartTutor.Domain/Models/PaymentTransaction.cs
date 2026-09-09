using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTutor.Domain.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }

        public int? ReportId { get; set; }

        public int? StudentId { get; set; }

        [Required]
        [StringLength(50)]
        public string Gateway { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountIn { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime TransactionTime { get; set; }

        public string? RawPayload { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual MonthlyReport? MonthlyReport { get; set; }

        public virtual Student? Student { get; set; }
    }
}
