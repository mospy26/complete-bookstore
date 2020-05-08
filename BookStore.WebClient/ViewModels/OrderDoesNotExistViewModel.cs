using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookStore.WebClient.ViewModels
{
    public class OrderDoesNotExistViewModel
    {
        public OrderDoesNotExistViewModel(int pOrderId)
        {
            OrderId = pOrderId;
        }

        public int OrderId { get; set; }
    }
}