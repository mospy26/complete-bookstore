using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;
using System.Transactions;
using Microsoft.Practices.ServiceLocation;
using DeliveryCo.MessageTypes;

namespace BookStore.Business.Components
{
    public class OrderProvider : IOrderProvider
    {
        public IEmailProvider EmailProvider
        {
            get { return ServiceLocator.Current.GetInstance<IEmailProvider>(); }
        }

        public IUserProvider UserProvider
        {
            get { return ServiceLocator.Current.GetInstance<IUserProvider>(); }
        }

        public List<int> GetOrders(int pUserId)
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                var lOrders = (from Order1 in lContainer.Orders.Include("OrderItems").Include("Customer").Include("OrderItems.Book")
                                       where Order1.Customer.Id == pUserId
                                       select Order1.Id).ToList<int>();
                return lOrders;

                //List<Order> lOrders = lContainer.Orders.Include("OrderItems").Include("OrderItems.Book").Include("OrderItems.Book.Stocks").Include("Customer").Where(s => s.Customer.Id.Equals(pUserId)).ToList<Order>();
                //lOrders.ForEach(o => { o.OrderItems.ToList().ForEach(oi => oi.Book = null); });
                //return lOrders;
            }
        }

        public void SubmitOrder(Entities.Order pOrder)
        {      
            using (TransactionScope lScope = new TransactionScope())
            {
                //LoadBookStocks(pOrder);
                //MarkAppropriateUnchangedAssociations(pOrder);

                using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
                {
                    try
                    {
                        lContainer.Users.Attach(pOrder.Customer);

                        pOrder.OrderNumber = Guid.NewGuid();
                        pOrder.Store = "OnLine";

                        // Book objects in pOrder are missing the link to their Stock tuple (and the Stock GUID field)
                        // so fix up the 'books' in the order with well-formed 'books' with 1:1 links to Stock tuples
                        foreach (OrderItem lOrderItem in pOrder.OrderItems)
                        {
                            int bookId = lOrderItem.Book.Id;
                            lOrderItem.Book = lContainer.Books.Where(book => bookId == book.Id).First();
                            lOrderItem.Book.Stocks = lContainer.Stocks.Where(stock => bookId == stock.Book.Id).ToList<Stock>();
                        }
                        // and update the stock levels
                        List<Tuple<Stock, OrderItem, int>> lConsumedStocks = pOrder.UpdateStockLevels();

                        // record the stocks that have been consumed
                        RecordPurchasedBooksFromStocks(lConsumedStocks, lContainer);

                        // add the modified Order tree to the Container (in Changed state)
                        lContainer.Orders.Add(pOrder);

                        // ask the Bank service to transfer fundss
                        TransferFundsFromCustomer(UserProvider.ReadUserById(pOrder.Customer.Id).BankAccountNumber, pOrder.Total ?? 0.0);

                        // ask the delivery service to organise delivery
                        PlaceDeliveryForOrder(pOrder);

                        // and save the order
                        lContainer.SaveChanges();
                        lScope.Complete();        
                    }
                    catch (Exception lException)
                    {
                        SendOrderErrorMessage(pOrder, lException);
                        IEnumerable<System.Data.Entity.Infrastructure.DbEntityEntry> entries =  lContainer.ChangeTracker.Entries();
                        throw;
                    }
                }
            }
            SendOrderPlacedConfirmation(pOrder);
        }

        public void CancelOrder(Entities.Order pOrder)
        {
            // TODO 
            using (TransactionScope lScope = new TransactionScope())
            {
                //LoadBookStocks(pOrder);
                //MarkAppropriateUnchangedAssociations(pOrder);

                using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
                {
                    try
                    {
                        List<OrderItem> orderItems = pOrder.OrderItems.ToList<OrderItem>();

                        // Restore stocks
                        RestoreStock(orderItems, lContainer);

                        lContainer.Orders.Remove(pOrder);

                        // ask the Bank service to transfer fundss
                        TransferFundsToCustomer(UserProvider.ReadUserById(pOrder.Customer.Id).BankAccountNumber, pOrder.Total ?? 0.0);

                        // Delete the delivery in the delivery table 
                        DeleteDelivery(pOrder.OrderNumber.ToString());
                    }
                    catch (Exception lException)
                    {
                        SendOrderErrorMessage(pOrder, lException);
                        IEnumerable<System.Data.Entity.Infrastructure.DbEntityEntry> entries = lContainer.ChangeTracker.Entries();
                        throw;
                    }
                }
            }
        }

        private void DeleteDelivery(string OrderNumber)
        {
            ExternalServiceFactory.Instance.DeliveryService.DeleteDelivery(OrderNumber);
        }

        //private void MarkAppropriateUnchangedAssociations(Order pOrder)
        //{
        //    pOrder.Customer.MarkAsUnchanged();
        //    pOrder.Customer.LoginCredential.MarkAsUnchanged();
        //    foreach (OrderItem lOrder in pOrder.OrderItems)
        //    {
        //        lOrder.Book.Stock.MarkAsUnchanged();
        //        lOrder.Book.MarkAsUnchanged();
        //    }
        //}

        private void LoadOptimalWarehouseStocks(Order pOrder)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                foreach (OrderItem lOrderItem in pOrder.OrderItems)
                {
                    lOrderItem.Book.Stocks = lContainer.Stocks.Where((pStock) => pStock.Book.Id == lOrderItem.Book.Id).ToList<Stock>();    
                }
            }
        }

        private void SendOrderErrorMessage(Order pOrder, Exception pException)
        {
            EmailProvider.SendMessage(new EmailMessage()
            {
                ToAddress = pOrder.Customer.Email,
                Message = "There was an error in processsing your order " + pOrder.OrderNumber + ": "+ pException.Message + ". Please contact Book Store"
            });
        }

        private void SendOrderPlacedConfirmation(Order pOrder)
        {
            EmailProvider.SendMessage(new EmailMessage()
            {
                ToAddress = pOrder.Customer.Email,
                Message = "Your order " + pOrder.OrderNumber + " has been placed"
            });
        }

        private void PlaceDeliveryForOrder(Order pOrder)
        {
            Delivery lDelivery = new Delivery() { DeliveryStatus = DeliveryStatus.Submitted, SourceAddress = "Book Store Address", DestinationAddress = pOrder.Customer.Address, Order = pOrder };

            Guid lDeliveryIdentifier = ExternalServiceFactory.Instance.DeliveryService.SubmitDelivery(new DeliveryInfo()
            { 
                OrderNumber = lDelivery.Order.OrderNumber.ToString(),  
                SourceAddress = lDelivery.SourceAddress,
                DestinationAddress = lDelivery.DestinationAddress,
                DeliveryNotificationAddress = "net.tcp://localhost:9010/DeliveryNotificationService"
            });

            lDelivery.ExternalDeliveryIdentifier = lDeliveryIdentifier;
            pOrder.Delivery = lDelivery;   
        }

        private void TransferFundsFromCustomer(int pCustomerAccountNumber, double pTotal)
        {
            try
            {
                ExternalServiceFactory.Instance.TransferService.Transfer(pTotal, pCustomerAccountNumber, RetrieveBookStoreAccountNumber());
            }
            catch
            {
                throw new Exception("Error when transferring funds for order.");
            }
        }

        private void TransferFundsToCustomer(int pCustomerAccountNumber, double pTotal)
        {
            try
            {
                ExternalServiceFactory.Instance.TransferService.Transfer(pTotal, RetrieveBookStoreAccountNumber(), pCustomerAccountNumber);
            } 
            catch
            {
                throw new Exception("Error transferring funds to customer");
            }
        }

        private int RetrieveBookStoreAccountNumber()
        {
            return 123;
        }

        private void RecordPurchasedBooksFromStocks(List<Tuple<Stock, OrderItem, int>> pConsumedStocks, BookStoreEntityModelContainer pContainer)
        {
            try
            {
                foreach (Tuple<Stock, OrderItem, int> consumedStock in pConsumedStocks)
                {
                    OrderStock orderStock = new OrderStock()
                    {
                        Quantity = consumedStock.Item3,
                        Stock = consumedStock.Item1,
                        OrderItem = consumedStock.Item2
                    };
                    pContainer.OrderStocks.Add(orderStock);
                }
            }
            catch (Exception lException)
            {
                throw;
            }
        }

        private void RestoreStock(List<OrderItem> orderItems, BookStoreEntityModelContainer pContainer)
        {

            List<int> orderIds = orderItems.Select(o => o.Id).ToList<int>();

            List<OrderStock> orderStocks = (from OrderStock1 in pContainer.OrderStocks.Include("OrderStock.Stock").Include("OrderStock.OrderItem")
                                            where orderIds.Contains(OrderStock1.OrderItem.Id)
                                            select OrderStock1).ToList<OrderStock>();

            foreach (OrderStock orderStock in orderStocks)
            {
                Stock stock = pContainer.Stocks.SingleOrDefault(r => r.Id == orderStock.Stock.Id);

                // the item was not chosen - should never reach this case but in case
                if (stock == null)
                {
                    continue;
                }

                stock.Quantity += orderStock.Quantity;
                pContainer.OrderStocks.Remove(orderStock);
                pContainer.Stocks.Attach(stock);
            }
        }
    }
}
