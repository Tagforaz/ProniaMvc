

namespace Pronia_MVC.ViewModels
{
    public class ShopVM
    {
        public List<GetProductVM> ProductVMs { get; set; }
        public List<GetCategoryVM> CategoryVMs { get; set; }
        public int? CategoryId { get; set; }
        public string? Search {  get; set; }
        public int Key { get; set; }
        public int TotalPage { get; set; }
        public int CurrentPage { get; set; }
    }
}
