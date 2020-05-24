using System;

namespace BookStore.Business.Entities
{
    public class OrderHasAlreadyBeenDeliveredException : Exception
    {
        public int OrderId { get; set; }
    }
}
