using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;
using Microsoft.Practices.ServiceLocation;
using System.Transactions;

namespace BookStore.Business.Components
{
    public class DeliveryNotificationProvider : IDeliveryNotificationProvider
    {
        public IEmailProvider EmailProvider
        {
            get { return ServiceLocator.Current.GetInstance<IEmailProvider>(); }
        }
        
        public void NotifyPickedUpOrder(Guid pDeliveryId)
        {
            Order lAffectedOrder = RetrieveDeliveryOrder(pDeliveryId);

            EmailProvider.SendMessage(new EmailMessage()
            {
                ToAddress = lAffectedOrder.Customer.Email,
                Message = "Your order " + lAffectedOrder.OrderNumber + " has been picked up and should be delivered soon!"
            });
        }

        public void NotifyOnDeliveryTruckOrder(Guid pDeliveryId)
        {
            Order lAffectedOrder = RetrieveDeliveryOrder(pDeliveryId);

            EmailProvider.SendMessage(new EmailMessage()
            {
                ToAddress = lAffectedOrder.Customer.Email,
                Message = "Your order " + lAffectedOrder.OrderNumber + " is on the delivery truck and on its way!"
            });
        }

        public void NotifyDeliveryCompletion(Guid pDeliveryId, Entities.DeliveryStatus status)
        {
            Order lAffectedOrder = RetrieveDeliveryOrder(pDeliveryId);

            // Delivery was deleted due to order cancellation
            if (lAffectedOrder == null)
            {
                return;
            }

            UpdateDeliveryStatus(pDeliveryId, status);
            if (status == Entities.DeliveryStatus.Delivered)
            {
                EmailProvider.SendMessage(new EmailMessage()
                {
                    ToAddress = lAffectedOrder.Customer.Email,
                    Message = "Our records show that your order " +lAffectedOrder.OrderNumber + " has been delivered. Thank you for shopping at Book store"
                });
            }
            if (status == Entities.DeliveryStatus.Failed)
            {
                EmailProvider.SendMessage(new EmailMessage()
                {
                    ToAddress = lAffectedOrder.Customer.Email,
                    Message = "Our records show that there was a problem " + lAffectedOrder.OrderNumber + " delivering your order. Please contact Book Store"
                });
            }
        }

        private void UpdateDeliveryStatus(Guid pDeliveryId, DeliveryStatus status)
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                Delivery lDelivery = lContainer.Deliveries.Where((pDel) => pDel.ExternalDeliveryIdentifier == pDeliveryId).FirstOrDefault();
                if (lDelivery != null)
                {
                    lDelivery.DeliveryStatus = status;
                    lContainer.SaveChanges();
                }
                lScope.Complete();
            }
        }

        private Order RetrieveDeliveryOrder(Guid pDeliveryId)
        {
 	        using(BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                Delivery lDelivery =  lContainer.Deliveries.Include("Order.Customer").Where((pDel) => pDel.ExternalDeliveryIdentifier == pDeliveryId).FirstOrDefault();

                // Order was cancelled
                if (lDelivery == null) return null;
                
                return lDelivery.Order;
            }
        }
    }


}
