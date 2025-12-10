namespace Pronia_MVC.ViewModels
{
    public class BasketVM
    {
        public List<BasketItemVM> BasketItemVMs { get; set; }
        public decimal Total {  get; set; }
    }
}
