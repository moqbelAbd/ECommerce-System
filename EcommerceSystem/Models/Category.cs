using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EcommerceSystem.Models
{
    public class Category
    {
        [Key]
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string categoryImagePath { get; set; }

        public bool IsDeleted { get; set; }

        public ICollection<SubCategory> subCategories { get; set; } = new HashSet<SubCategory>();

    }
}
