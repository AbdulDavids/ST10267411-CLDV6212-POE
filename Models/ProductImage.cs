using System;

namespace CLDV6212_POE.Models
{
    public class ProductImage
    {
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
        public string Url { get; set; } // Add this property
    }
}