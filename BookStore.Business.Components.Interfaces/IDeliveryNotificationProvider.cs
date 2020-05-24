using BookStore.Business.Entities;
using System;

namespace BookStore.Business.Components.Interfaces
{
    public interface IDeliveryNotificationProvider
    {

        void NotifyDeliveryCompletion(Guid pDeliveryId, DeliveryStatus status);

        bool NotifyPickedUpOrder(Guid pDeliveryId);

        bool NotifyOnDeliveryTruckOrder(Guid pDeliveryId);
    }
}
