using Pronia_MVC.Models;
using System.ComponentModel.DataAnnotations;

namespace Pronia_MVC.ViewModels
{
    public class UpdateProductVM
    {
        public string Name { get; set; }
        [Required]
        [Range(0.01D, (double)decimal.MaxValue, ErrorMessage = "The value  must be a positive integer ")]
        public decimal? Price { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        public IFormFile? PrimaryPhoto { get; set; }
        public IFormFile? SecondaryPhoto { get; set; }
        [Required]
        public int? CategoryId { get; set; }
        public List<int>? ColorIds { get; set; }
        public List<int>? SizeIds { get; set; }
        public List<int>? TagIds { get; set; }
        public List<Category>? Categories { get; set; }
        public List<Size>? Sizes { get; set; }
        public List<Color>? Colors { get; set; }
        public List<Tag>? Tags { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
    }
}
