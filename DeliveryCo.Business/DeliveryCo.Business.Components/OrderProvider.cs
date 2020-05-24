
using DeliveryCo.Business.Components.Interfaces;
using System;

namespace DeliveryCo.Business.Components
{
    public class OrderProvider : IOrderProvider
    {
        public void NotifyMessage(String message)
        {
            ExternalServiceFactory.Instance.OrderService.GetNotificationFromDeliveryCo(message);
        }
    }
}
