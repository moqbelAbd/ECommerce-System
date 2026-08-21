using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class ProductSubCategory
    {

        [Key]
        public Guid ProductSubCategoryId { get; set; }

        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public Guid SubCategoryId { get; set; }

        [ForeignKey("SubCategoryId")]
        public SubCategory? SubCategory { get; set; }

        //public ICollection<SubCategory> SubCategories { get; set; } = new HashSet<SubCategory>();
    }
}
