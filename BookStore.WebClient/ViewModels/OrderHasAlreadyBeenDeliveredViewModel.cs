namespace BookStore.WebClient.ViewModels
{
    public class OrderHasAlreadyBeenDeliveredViewModel
    {
        public OrderHasAlreadyBeenDeliveredViewModel(int pOrderId)
        {
            OrderId = pOrderId;
        }

        public int OrderId { get; set; }
    }
}