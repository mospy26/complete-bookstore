using System.Collections.Generic;
using BookStore.Business.Entities;

namespace BookStore.Business.Components.Interfaces
{
    public interface IOrderProvider
    {
        string SubmitOrder(Order pOrder);
        void CancelOrder(int pOrderId);

        List<int> GetOrders(int pUserId);
    }
}
