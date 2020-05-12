using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
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
