using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTutor.Domain.Models
{
    public class User
    {
        public int Id { get; set; }

        public int RoleId { get; set; } = 1;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string BankCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string BankAccountNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string BankAccountName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? WebhookToken { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual Role? Role { get; set; }

        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

        public virtual ICollection<Student> Students { get; set; } = new List<Student>();

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public virtual ICollection<ResetPasswordToken> ResetPasswordTokens { get; set; } = new List<ResetPasswordToken>();
    }
}
