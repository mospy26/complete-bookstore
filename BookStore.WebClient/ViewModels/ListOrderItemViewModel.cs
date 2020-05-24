using BookStore.Services.Interfaces;
using BookStore.Services.MessageTypes;
using System.Collections.Generic;

namespace BookStore.WebClient.ViewModels
{
    public class ListOrderItemViewModel
    {
        public ListOrderItemViewModel(User pUser)
        {
            UserId = pUser.Id;
        }

        public int UserId { get; set; }

        private IOrderService OrderService
        {
            get
            {
                return ServiceFactory.Instance.OrderService;
            }
        }

        public List<int> OrderItems
        {
            get
            {
                return OrderService.GetOrders(UserId);
            }
        }
    }
}