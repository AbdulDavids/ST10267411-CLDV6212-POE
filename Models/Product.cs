using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE.Models
{
    public class Product
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }
    }
}