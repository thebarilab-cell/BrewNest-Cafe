using System.ComponentModel.DataAnnotations;

namespace BrewNestCafe.Models
{
    public class Gallery
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Image URL is required")]
        [StringLength(200)]
        public string ImageUrl { get; set; }

        [StringLength(100)]
        public string? Title { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}