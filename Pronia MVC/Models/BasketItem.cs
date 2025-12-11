namespace Pronia_MVC.Models
{
    public class BasketItem:BaseEntity
    {
        public int Count { get; set; }
        

        //relational
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

    }
}
