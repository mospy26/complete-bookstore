using DeliveryCo.Services.Interfaces;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;

namespace BookStore.Services
{
    public class DeliveryNotificationService : IDeliveryNotificationService
    {
        IDeliveryNotificationProvider Provider
        {
            get { return ServiceLocator.Current.GetInstance<IDeliveryNotificationProvider>(); }
        }

        public void NotifyDeliveryCompletion(Guid pDeliveryId, DeliveryInfoStatus status)
        {
            Provider.NotifyDeliveryCompletion(pDeliveryId, GetDeliveryStatusFromDeliveryInfoStatus(status));
        }

        public bool NotifyPickedUpOrder(Guid pDeliveryId)
        {
            return Provider.NotifyPickedUpOrder(pDeliveryId);
        }

        public bool NotifyOnDeliveryTruckOrder(Guid pDeliveryId)
        {
            return Provider.NotifyOnDeliveryTruckOrder(pDeliveryId);
        }

        private DeliveryStatus GetDeliveryStatusFromDeliveryInfoStatus(DeliveryInfoStatus status)
        {
            if (status == DeliveryInfoStatus.Delivered)
            {
                return DeliveryStatus.Delivered;
            }
            else if (status == DeliveryInfoStatus.Failed)
            {
                return DeliveryStatus.Failed;
            }
            else if (status == DeliveryInfoStatus.Submitted)
            {
                return DeliveryStatus.Submitted;
            }
            else
            {
                throw new Exception("Unexpected delivery status received");
            }
        }

    }
}
