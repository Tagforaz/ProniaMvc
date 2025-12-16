using Pronia_MVC.Utilities.Enums;

namespace Pronia_MVC.Models
{
    public class Order:BaseEntity
    {

        public string Address { get; set; }
        public decimal Total  { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime? CompletedAt { get; set; }

        //relational
        public string AppUserId { get; set; }
        public AppUser AppUser {  get; set; }
        public List<OrderItem> OrderItems { get; set; }

    }
}
