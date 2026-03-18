using System.ComponentModel.DataAnnotations;

namespace BrewNestCafe.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50)]
        public string Name { get; set; }

        public virtual ICollection<MenuItem>? MenuItems { get; set; }
    }
}