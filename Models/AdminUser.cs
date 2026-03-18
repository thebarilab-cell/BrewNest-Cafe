using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BrewNestCafe.Models
{
    public class AdminUser : IdentityUser
    {
        [StringLength(100)]
        public string? FullName { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }
    }
}