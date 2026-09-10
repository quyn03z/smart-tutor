using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTutor.Domain.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string Token { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime? RevokedAt { get; set; }

        public virtual User? User { get; set; }
    }
}
