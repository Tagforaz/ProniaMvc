using Pronia_MVC.Models;

namespace Pronia_MVC.ViewModels
{
    public class GetCategoryVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public List<Product>? Products { get; set; }
    }
}
