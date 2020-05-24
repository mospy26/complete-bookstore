using System;

namespace BookStore.Business.Entities
{
    public class InsufficientStockException : Exception
    {
        public String ItemName { get; set; }
    }
}
