using System;

namespace BookStore.Services.MessageTypes
{
    public class InsufficientStockFault
    {
        public String ItemName { get; set; }
    }
}
