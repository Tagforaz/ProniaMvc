using Pronia_MVC.Models;

namespace Pronia_MVC.ViewModels
{
    public class DetailsVM
    {
        public Product Product { get; set; }
        public List<Product> RelatedProducts { get; set; }
    }
}
