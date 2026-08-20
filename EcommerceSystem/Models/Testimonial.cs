using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class Testimonial
    {

        [Key]
        public Guid TestimonialId { get; set; }

        [Required]
        public string CustomerTestimonial { get; set; } = string.Empty;

        public bool IsApproved {  get; set; }

        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }
}
