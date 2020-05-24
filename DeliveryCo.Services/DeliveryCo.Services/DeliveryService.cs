﻿using System;
using DeliveryCo.Services.Interfaces;
using DeliveryCo.Business.Components.Interfaces;
using System.ServiceModel;
using Microsoft.Practices.ServiceLocation;
using DeliveryCo.MessageTypes;

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
