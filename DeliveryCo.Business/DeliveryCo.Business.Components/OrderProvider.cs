
using System;
using DeliveryCo.Business.Components.Interfaces;

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
