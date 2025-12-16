namespace Pronia_MVC.ViewModels
{
    public class PaginatedItemsVM<T> where T : class,new()
    {
        public List<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPage { get; set; }
    }
}
