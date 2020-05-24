namespace BookStore.Services.MessageTypes
{
    public class OrderItem : MessageType
    {
        public Book Book { get; set; }
        public int Quantity { get; set; }
    }
}
