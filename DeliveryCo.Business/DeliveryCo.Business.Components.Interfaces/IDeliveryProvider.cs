using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeliveryCo.Business.Components.Interfaces
{
    public interface IDeliveryProvider
    {
        Guid SubmitDelivery(DeliveryCo.Business.Entities.DeliveryInfo pDeliveryInfo, List<Tuple<String, List<String>>> pOrderItems);

        bool DeleteDelivery(String pDeliveryInfo);
    }
}
