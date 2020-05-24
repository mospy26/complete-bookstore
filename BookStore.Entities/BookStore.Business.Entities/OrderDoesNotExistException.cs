using System;

namespace BookStore.Business.Entities
{
    public class OrderDoesNotExistException : Exception
    {
        public int OrderId { get; set; }
    }
}
