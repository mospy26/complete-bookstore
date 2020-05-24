using System;
using System.ServiceModel;
using DeliveryCo.MessageTypes;

namespace DeliveryCo.Services.Interfaces
{
    [ServiceContract]
    public interface IDeliveryService
    {
        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        Guid SubmitDelivery(DeliveryInfo pDeliveryInfo, OrderInfo pOrderItemToWarehouses);

        [OperationContract]
        Boolean DeleteDelivery(String OrderNumber);
    }
}
