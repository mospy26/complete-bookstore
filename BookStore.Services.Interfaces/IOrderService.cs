using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using BookStore.Services.MessageTypes;

namespace BookStore.Services.Interfaces
{
    [ServiceContract]
    public interface IOrderService
    {
        [OperationContract]
        [FaultContract(typeof(InsufficientStockFault))]
        string SubmitOrder(Order pOrder);

        [OperationContract]
        List<int> GetOrders(int pUserId);

        [OperationContract]
        [FaultContract(typeof(OrderDoesNotExistFault))]
        [FaultContract(typeof(OrderHasAlreadyBeenDeliveredFault))]
        void CancelOrder(int pOrderId);

        [OperationContract]
        void GetNotificationFromDeliveryCo(String message);
    }
}
