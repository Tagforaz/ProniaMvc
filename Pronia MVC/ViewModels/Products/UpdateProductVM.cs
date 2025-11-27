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
        [Required]
        public int? CategoryId { get; set; }
        public List<Category>? Categories { get; set; }
    }
}
