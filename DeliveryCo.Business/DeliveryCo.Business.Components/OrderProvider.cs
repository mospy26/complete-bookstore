
using System;using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
