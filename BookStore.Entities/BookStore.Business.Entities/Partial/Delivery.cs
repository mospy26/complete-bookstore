namespace BookStore.Business.Entities
{
    public enum DeliveryStatus { Submitted, Delivered, Failed }

    public partial class Delivery
    {
        public DeliveryStatus DeliveryStatus
        {
            get
            {
                return (DeliveryStatus)this.Status;
            }
            set
            {
                this.Status = (int)value;
            }
        }
    }
}
