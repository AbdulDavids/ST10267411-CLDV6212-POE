using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE.Models
{
    public class Order
    {
        public string Id { get; set; }

        [Required]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; } // Add this property

        public List<Product> Products { get; set; } = new List<Product>();

        [Required]
        public string Status { get; set; }

        public DateTime TimeQueued { get; set; }
    }
}