using System.ComponentModel.DataAnnotations;

namespace BrewNestCafe.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000)]
        public string Message { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        public bool IsApproved { get; set; } = false;

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
    }
}