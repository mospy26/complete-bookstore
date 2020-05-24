using BookStore.Business.Entities;
using System.Collections.Generic;

namespace BookStore.Business.Components.Interfaces
{
    public interface IOrderProvider
    {
        string SubmitOrder(Order pOrder);
        void CancelOrder(int pOrderId);

        List<int> GetOrders(int pUserId);
    }
}
