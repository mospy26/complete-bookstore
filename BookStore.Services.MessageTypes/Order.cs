using System.Collections.Generic;

namespace BookStore.Services.MessageTypes
{
    public class Order : MessageType
    {
        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        public List<OrderItem> OrderItems { get; set; }
        public double Total { get; set; }
        public System.DateTime OrderDate { get; set; }
        public int Status { get; set; }
        public User Customer { get; set; }
    }
}
