using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTutor.Domain.Models
{
    public class ClassEnrollment
    {
        public int Id { get; set; }

        public int ClassId { get; set; }

        public int StudentId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustomFee { get; set; }

        [Column(TypeName = "date")]
        public DateTime JoinedDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        public virtual Class? Class { get; set; }

        public virtual Student? Student { get; set; }
    }
}
