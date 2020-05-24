using DeliveryCo.Business.Components.Interfaces;
using DeliveryCo.MessageTypes;
using DeliveryCo.Services.Interfaces;
using Microsoft.Practices.ServiceLocation;
using System;
using System.ServiceModel;

namespace DeliveryCo.Services
{
    public class DeliveryService : IDeliveryService
    {
        private IDeliveryProvider DeliveryProvider
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IDeliveryProvider>();
            }
        }

        public bool DeleteDelivery(String OrderNumber)
        {
            return DeliveryProvider.DeleteDelivery(OrderNumber);
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public Guid SubmitDelivery(DeliveryInfo pDeliveryInfo, OrderInfo pOrderItemToWarehouses)
        {
            return DeliveryProvider.SubmitDelivery(
                MessageTypeConverter.Instance.Convert<DeliveryCo.MessageTypes.DeliveryInfo,
                DeliveryCo.Business.Entities.DeliveryInfo>(pDeliveryInfo), pOrderItemToWarehouses.OrderItem
            );
        }
    }
}
