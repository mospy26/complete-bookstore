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

        public Guid SubmitDelivery(DeliveryCo.Business.Entities.DeliveryInfo pDeliveryInfo, List<Tuple<String, List<String>>> pOrderItems)
        {
            using(TransactionScope lScope = new TransactionScope())
            using(DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                pDeliveryInfo.DeliveryIdentifier = Guid.NewGuid();
                pDeliveryInfo.Status = 0;
                lContainer.DeliveryInfo.Add(pDeliveryInfo);
                lContainer.SaveChanges();
                ThreadPool.QueueUserWorkItem(new WaitCallback((pObj) => ScheduleDelivery(pDeliveryInfo, pOrderItems)));
                lScope.Complete();
            }
            return pDeliveryInfo.DeliveryIdentifier;
        }

        private void ScheduleDelivery(DeliveryInfo pDeliveryInfo, List<Tuple<String, List<String>>> pOrderItems)
        {
            // Pick up notification
            Console.WriteLine("Request for delivering items received! Delivering from warehouse address: " + pDeliveryInfo.SourceAddress + " to " + pDeliveryInfo.DestinationAddress);

            //notify received request - send a request to the BookStore stating that you have received the request

            Thread.Sleep(3000);

            Console.WriteLine("Delivering to" + pDeliveryInfo.DestinationAddress);
            Console.WriteLine();

            foreach (Tuple<string, List<String>> e in pOrderItems)
            {
                Console.WriteLine("Book " + e.Item1 + " dispatching from warehouses:");
                foreach (String f in e.Item2)
                {
                    Console.WriteLine(f);
                }
                Console.WriteLine();
            }

            // notify goods have been picked up - send a request to the BookStore stating that you have picked up the books from those Warehouses

            Thread.Sleep(3000);

            //notify that goods are on their way - tell the BookStore that books are on the way

            Console.WriteLine("Delivering to " + pDeliveryInfo.DestinationAddress);

            // notify order is completed - tell the BookStore that the books have been delivered

            //notifying of delivery completion
            using (TransactionScope lScope = new TransactionScope())
            using (DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                pDeliveryInfo.Status = 1;
                IDeliveryNotificationService lService = DeliveryNotificationServiceFactory.GetDeliveryNotificationService(pDeliveryInfo.DeliveryNotificationAddress);
                lService.NotifyDeliveryCompletion(pDeliveryInfo.DeliveryIdentifier, DeliveryInfoStatus.Delivered);
            }

        }
    }
}
