using Microsoft.CodeAnalysis;
using System.ComponentModel.DataAnnotations;

namespace EcommerceSystem.Models
{
    public class SubCategory
    {

        [Key]
        public Guid SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string SubCategoryImagePath { get; set; }

        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
}
