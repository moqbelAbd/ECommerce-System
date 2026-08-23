using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EcommerceSystem.Models
{
    public class Category
    {
        [Key]
        public Guid CategoryId { get; set; }

        [Required]
        [MaxLength(20)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        public string CategoryImagePath { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public ICollection<SubCategory> SubCategories { get; set; } = new HashSet<SubCategory>();

    }
}
