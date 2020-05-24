using BookStore.Services.MessageTypes;
using System;
using System.Collections.Generic;
using System.ServiceModel;

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
