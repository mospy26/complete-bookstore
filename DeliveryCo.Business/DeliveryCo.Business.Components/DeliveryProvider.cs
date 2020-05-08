using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeliveryCo.Business.Components.Interfaces;
using System.Transactions;
using DeliveryCo.Business.Entities;
using System.Threading;
using DeliveryCo.Services.Interfaces;


namespace DeliveryCo.Business.Components
{
    public class DeliveryProvider : IDeliveryProvider
    {
        public bool DeleteDelivery(String OrderNumber)
        {

            using(DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                try
                {
                    DeliveryInfo deleteDelivery = lContainer.DeliveryInfo.Where<DeliveryInfo>(s => s.OrderNumber.Equals(OrderNumber)).First();
                    lContainer.DeliveryInfo.Remove(deleteDelivery);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }

        }

        public Guid SubmitDelivery(DeliveryCo.Business.Entities.DeliveryInfo pDeliveryInfo)
        {
            using(TransactionScope lScope = new TransactionScope())
            using(DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                pDeliveryInfo.DeliveryIdentifier = Guid.NewGuid();
                pDeliveryInfo.Status = 0;
                lContainer.DeliveryInfo.Add(pDeliveryInfo);
                lContainer.SaveChanges();
                ThreadPool.QueueUserWorkItem(new WaitCallback((pObj) => ScheduleDelivery(pDeliveryInfo)));
                lScope.Complete();
            }
            return pDeliveryInfo.DeliveryIdentifier;
        }

        private void ScheduleDelivery(DeliveryInfo pDeliveryInfo)
        {
            // Pick up notification
            Console.WriteLine("Request for delivering items received! Delivering from warehouse address: " + pDeliveryInfo.SourceAddress + " to " + pDeliveryInfo.DestinationAddress);

            // notify received request
            ExternalServiceFactory.Instance.OrderService.GetNotificationFromDeliveryCo("Notification from DeliveryCo: Received request to deliver books from warehouse address: " + pDeliveryInfo.SourceAddress + " to " + pDeliveryInfo.DestinationAddress);

            Thread.Sleep(3000);

            // notify goods have been picked up
            ExternalServiceFactory.Instance.OrderService.GetNotificationFromDeliveryCo("Notification from DeliveryCo: Books for delivery number: " + pDeliveryInfo.DeliveryIdentifier + " has been picked up");

            bool lEmailResult = true;
            using (TransactionScope lScope = new TransactionScope())
            using (DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                IDeliveryNotificationService lService = DeliveryNotificationServiceFactory.GetDeliveryNotificationService(pDeliveryInfo.DeliveryNotificationAddress);
                lEmailResult = lService.NotifyPickedUpOrder(pDeliveryInfo.DeliveryIdentifier);
            }

            // Order got cancelled
            if (!lEmailResult)
            {
                Console.WriteLine("Delivery number " + pDeliveryInfo.DeliveryIdentifier + " was cancelled by the customer.");
                ExternalServiceFactory.Instance.OrderService.GetNotificationFromDeliveryCo("Notification from DeliveryCo: Delivery number " + pDeliveryInfo.DeliveryIdentifier + " was cancelled by the customer.");
                return;
            }

            Thread.Sleep(3000);

            // notify that goods are on their way
            ExternalServiceFactory.Instance.OrderService.GetNotificationFromDeliveryCo("Notification from DeliveryCo: Books for delivery number " + pDeliveryInfo.DeliveryIdentifier + " are on their way to the customer at address " + pDeliveryInfo.DestinationAddress);

            using (TransactionScope lScope = new TransactionScope())
            using (DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                IDeliveryNotificationService lService = DeliveryNotificationServiceFactory.GetDeliveryNotificationService(pDeliveryInfo.DeliveryNotificationAddress);
                lEmailResult = lService.NotifyOnDeliveryTruckOrder(pDeliveryInfo.DeliveryIdentifier);
            }

            // Order got cancelled
            if (!lEmailResult)
            {
                Console.WriteLine("Delivery number " + pDeliveryInfo.DeliveryIdentifier + " was cancelled by the customer.");
                ExternalServiceFactory.Instance.OrderService.GetNotificationFromDeliveryCo("Notification from DeliveryCo: Delivery number " + pDeliveryInfo.DeliveryIdentifier + " was cancelled by the customer.");
                return;
            }

            Console.WriteLine("Delivering to " + pDeliveryInfo.DestinationAddress);

            //notifying of delivery completion
            using (TransactionScope lScope = new TransactionScope())
            using (DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                pDeliveryInfo.Status = 1;
                IDeliveryNotificationService lService = DeliveryNotificationServiceFactory.GetDeliveryNotificationService(pDeliveryInfo.DeliveryNotificationAddress);
                lService.NotifyDeliveryCompletion(pDeliveryInfo.DeliveryIdentifier, DeliveryInfoStatus.Delivered);
            }

            // notify order is completed to the BookStore
            ExternalServiceFactory.Instance.OrderService.GetNotificationFromDeliveryCo("Notification from DeliveryCo: Books for delivery number: " + pDeliveryInfo.DeliveryIdentifier + " were delivered successfully!");
        }
    }
}
