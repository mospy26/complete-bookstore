﻿using System;
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
        
        public bool NotifyPickedUpOrder(Guid pDeliveryId)
        {
            Order lAffectedOrder = RetrieveDeliveryOrder(pDeliveryId);

            // Order got cancelled before being delivered
            if (lAffectedOrder == null)
            {
                Console.WriteLine("============PickUp Order Notification============");
                Console.WriteLine("Order Number: " + lAffectedOrder.OrderNumber);
                Console.WriteLine("Date: " + lAffectedOrder.OrderDate);
                Console.WriteLine("Status: FAILED");
                Console.WriteLine("=================================================" + "\n");
                return false;
            }
            EmailProvider.SendMessage(new EmailMessage()
            {
                ToAddress = lAffectedOrder.Customer.Email,
                Message = "Your order " + lAffectedOrder.OrderNumber + " has been picked up and should be delivered soon!"
            });

            Console.WriteLine("============PickUp Order Notification============");
            Console.WriteLine("Order Number: " + lAffectedOrder.OrderNumber);
            Console.WriteLine("Date: " + lAffectedOrder.OrderDate);
            Console.WriteLine("Status: SUCCESS");
            Console.WriteLine("=================================================" + "\n");

            return true;
        }

        public bool NotifyOnDeliveryTruckOrder(Guid pDeliveryId)
        {
            Order lAffectedOrder = RetrieveDeliveryOrder(pDeliveryId);

            // Order got cancelled before being delivered
            if (lAffectedOrder == null)
            {
                Console.WriteLine("============Delivery Truck Notification============");
                Console.WriteLine("Order Number: " + lAffectedOrder.OrderNumber);
                Console.WriteLine("Date: " + lAffectedOrder.OrderDate);
                Console.WriteLine("Status: FAILED");
                Console.WriteLine("===================================================" + "\n");
                return false;
            }
            EmailProvider.SendMessage(new EmailMessage()
            {
                ToAddress = lAffectedOrder.Customer.Email,
                Message = "Your order " + lAffectedOrder.OrderNumber + " is on the delivery truck and on its way!"
            });

            Console.WriteLine("============Delivery Truck Notification============");
            Console.WriteLine("Order Number: " + lAffectedOrder.OrderNumber);
            Console.WriteLine("Date: " + lAffectedOrder.OrderDate);
            Console.WriteLine("Status: SUCCESS");
            Console.WriteLine("===================================================" + "\n");

            return true;
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
                Console.WriteLine("============Delivery Complete Notification============");
                Console.WriteLine("Order Number: " + lAffectedOrder.OrderNumber);
                Console.WriteLine("Date: " + lAffectedOrder.OrderDate);
                Console.WriteLine("Status: SUCCESS");
                Console.WriteLine("======================================================" + "\n");
            }
            if (status == Entities.DeliveryStatus.Failed)
            {
                EmailProvider.SendMessage(new EmailMessage()
                {
                    ToAddress = lAffectedOrder.Customer.Email,
                    Message = "Our records show that there was a problem " + lAffectedOrder.OrderNumber + " delivering your order. Please contact Book Store"
                });
                Console.WriteLine("============Delivery Complete Notification============");
                Console.WriteLine("Order Number: " + lAffectedOrder.OrderNumber);
                Console.WriteLine("Date: " + lAffectedOrder.OrderDate);
                Console.WriteLine("Status: FAILED");
                Console.WriteLine("======================================================" + "\n");
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
                if (lDelivery == null)
                {
                    Console.WriteLine("============Retrieve Deliver Order============");
                    Console.WriteLine("Order Number: " + pDeliveryId);
                    Console.WriteLine("Status: FAILED");
                    Console.WriteLine("==============================================" + "\n");
                    return null;
                }
                Console.WriteLine("============Retrieve Deliver Order============");
                Console.WriteLine("Order Number: " + pDeliveryId);
                Console.WriteLine("Status: SUCCESS");
                Console.WriteLine("==============================================" + "\n");

                return lDelivery.Order;
            }
        }
    }


}
