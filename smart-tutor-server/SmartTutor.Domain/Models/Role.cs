using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartTutor.Domain.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
