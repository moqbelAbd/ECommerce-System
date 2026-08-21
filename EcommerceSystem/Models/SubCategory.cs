using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class SubCategory
    {

        [Key]
        public Guid SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string SubCategoryImagePath { get; set; }

        public bool IsDeleted { get; set; }

        public Guid? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
}
