using System;
using System.Collections.Generic;

namespace DeliveryCo.Business.Components.Interfaces
{
    public interface IDeliveryProvider
    {
        Guid SubmitDelivery(DeliveryCo.Business.Entities.DeliveryInfo pDeliveryInfo, List<Tuple<String, List<String>>> pOrderItems);

        bool DeleteDelivery(String pDeliveryInfo);
    }
}
