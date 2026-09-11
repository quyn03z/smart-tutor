using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTutor.Domain.Models
{
    public class ResetPasswordToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(256)]
        public string ResetToken { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime ExpiredAt { get; set; }

        public bool isUsed { get; set; } = false;

        [Column(TypeName = "datetime2")]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
    }
}
