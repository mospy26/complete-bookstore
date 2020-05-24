using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;
using System.Transactions;
using Microsoft.Practices.ServiceLocation;
using DeliveryCo.MessageTypes;
using System.Collections;
using System.ServiceModel;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Threading;

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
                var lOrders = (from Order1 in lContainer.Orders.Include("Delivery")
                               where Order1.Customer.Id == pUserId
                               select Order1);

                List<int> lOrderIds = new List<int>();

                foreach (Order order in lOrders)
                {
                    if (order.Delivery == null) lOrderIds.Add(order.Id);
                }
                return lOrderIds;
            }
        }

        public string SubmitOrder(Entities.Order pOrder)
        {
            string lCustomerEmail = "";
            Guid lOrderId = Guid.Empty;
            using (TransactionScope lScope = new TransactionScope())
            {
                //LoadBookStocks(pOrder);
                //MarkAppropriateUnchangedAssociations(pOrder);
                string lResult = "";

                using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
                {
                    try
                    {
                        lContainer.Users.Attach(pOrder.Customer);

                        pOrder.OrderNumber = Guid.NewGuid();
                        pOrder.Store = "OnLine";
                        lCustomerEmail = pOrder.Customer.Email;
                        lOrderId = pOrder.OrderNumber;

                        // Book objects in pOrder are missing the link to their Stock tuple (and the Stock GUID field)
                        // so fix up the 'books' in the order with well-formed 'books' with 1:1 links to Stock tuples
                        foreach (OrderItem lOrderItem in pOrder.OrderItems)
                        {
                            int bookId = lOrderItem.Book.Id;
                            lOrderItem.Book = lContainer.Books.Where(book => bookId == book.Id).First();
                            lOrderItem.Book.Stocks = lContainer.Stocks.Where(stock => bookId == stock.Book.Id).ToList<Stock>();
                        }

                        List<Warehouse> bestWares = LoadOptimalWarehouseStocks(pOrder);

                        // cannot satisfy
                        if(bestWares.Count == 0)
                        {
                            throw new Exception("Insufficient stock");
                        }

                        // and update the stock levels
                        List<Tuple<Stock, OrderItem, int>> lConsumedStocks = pOrder.UpdateStockLevels(bestWares, lContainer);

                        // record the stocks that have been consumed
                        RecordPurchasedBooksFromStocks(lConsumedStocks, lContainer);

                        // add the modified Order tree to the Container (in Changed state)
                        lContainer.Orders.Add(pOrder);

                        // ask the Bank service to transfer fundss
                        lResult = TransferFundsFromCustomer(UserProvider.ReadUserById(pOrder.Customer.Id).BankAccountNumber, pOrder.Total ?? 0.0);
                        
                        if (!lResult.Equals("Transfer Success"))
                        {
                            // Email the user about the cause of error through this exception
                            throw new Exception(lResult);
                        }

                        Console.WriteLine("=============Order Submit=============");
                        Console.WriteLine("Order ID: " + pOrder.Id);
                        Console.WriteLine("Status: SUCCESS");
                        Console.WriteLine("Time: " + DateTime.Now);
                        Console.WriteLine("======================================");
                        Console.WriteLine(" ");

                        // and save the order
                        lContainer.SaveChanges();
                        lScope.Complete();        
                    }
                    catch (Exception lException)
                    {
                        Console.WriteLine("=============Order Submit=============");
                        Console.WriteLine("Order ID: " + pOrder.Id);
                        Console.WriteLine("Status: FAILED");
                        Console.WriteLine("Time: " + DateTime.Now);
                        Console.WriteLine("======================================");
                        Console.WriteLine(" ");

                        // need to rollback bank transfer if the transfer happened
                        if (lResult == "Transfer Success")
                        {
                            Console.WriteLine("=============Calling BANK - Rollback=============");
                            Console.WriteLine("Intiating ROLLBACK on bank trasnfer");
                            Console.WriteLine("Order ID: " + pOrder.Id);
                            Console.WriteLine("Acc Number: " + UserProvider.ReadUserById(pOrder.Customer.Id).BankAccountNumber);
                            Console.WriteLine("Total: " + pOrder.Total);
                            Console.WriteLine("Time: " + DateTime.Now);
                            Console.WriteLine("======================================");
                            Console.WriteLine(" ");
                            TransferFundsToCustomer(UserProvider.ReadUserById(pOrder.Customer.Id).BankAccountNumber, pOrder.Total ?? 0.0);
                        }
                        SendOrderErrorMessage(pOrder, lException);
                        IEnumerable<System.Data.Entity.Infrastructure.DbEntityEntry> entries =  lContainer.ChangeTracker.Entries();
                        return lResult;
                    }
                }
            }

            SendOrderConfirmedConfirmation(pOrder);

            // User has 20 seconds to cancel the order
            Thread.Sleep(20000);

            using (TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                try
                {
                    Order lOrder = lContainer.Orders.Where(o => o.Id == pOrder.Id).FirstOrDefault<Order>();
                    if (lOrder == null) throw new Exception("Cannot place order as your order was cancelled");
                    CreateDelivery(lOrder);
                    lContainer.SaveChanges();
                    lScope.Complete();
                } catch (Exception lException)
                {
                    SendOrderDeletedErrorMessage(lCustomerEmail, lOrderId);
                    return "Order Failed";
                }
            }

            // 5 seconds later the order will be scheduled for delivery
            Thread.Sleep(5000);

            using (TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer()) 
            {
                try
                {
                    Order lOrder = lContainer.Orders.Where(o => o.Id == pOrder.Id).FirstOrDefault<Order>();
                    if (lOrder == null) throw new Exception("Cannot place order as your order was cancelled");
                    Delivery lDelivery = lContainer.Deliveries.Where(d => d.Order.Id == lOrder.Id).FirstOrDefault<Delivery>();
                    PlaceDeliveryForOrder(lOrder, lDelivery);
                    lContainer.SaveChanges();
                    lScope.Complete();
                } 
                catch (Exception lException)
                {
                    SendOrderDeletedErrorMessage(lCustomerEmail, lOrderId);
                    return "Order Failed";
                }
            }

            SendOrderPlacedConfirmation(pOrder);
            return "Order Submitted";
        }

        private void CreateDelivery(Order pOrder)
        {
            // Notify DeliveryCo that books are ready to pick up
            HashSet<String> lAddress = new HashSet<String>();
            GetDeliveryAddress(lAddress, pOrder);
            String lSourceAddress = String.Join(", ", lAddress);

            Delivery lDelivery = new Delivery()
            {
                DeliveryStatus = DeliveryStatus.Submitted,
                SourceAddress = lSourceAddress,
                DestinationAddress = pOrder.Customer.Address,
                Order = pOrder
            };

            pOrder.Delivery = lDelivery;
        }

        public void CancelOrder(int pOrderId)
        {
            String customerEmail = "";
            Guid orderNumber = Guid.Empty;
            
            using (TransactionScope lScope = new TransactionScope())
            {
                //LoadBookStocks(pOrder);
                //MarkAppropriateUnchangedAssociations(pOrder);

                using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
                {
                    Order lOrder = lContainer.Orders.FirstOrDefault(o => o.Id == pOrderId);
                    string result = "";
                    try
                    {

                        if (lOrder == null) throw new OrderDoesNotExistException();
                        if (lOrder.Delivery != null) throw new OrderHasAlreadyBeenDeliveredException();

                        customerEmail = lOrder.Customer.Email;
                        orderNumber = lOrder.OrderNumber;

                        List<OrderItem> orderItems = lOrder.OrderItems.ToList<OrderItem>();

                        Console.WriteLine("================Cancel Order=================");
                        Console.WriteLine("             Order ID: " + pOrderId);
                        Console.WriteLine("             CustomerID: " + lOrder.Customer.Id);
                        Console.WriteLine("             Name: " + lOrder.Customer.Name);
                        Console.WriteLine("             Address: " + lOrder.Customer.Address);
                        Console.WriteLine("             Time: " + DateTime.Now);
                        Console.WriteLine("Items Restored: ");
                        
                        foreach(OrderItem lOrderItem in orderItems)
                        {
                            Console.WriteLine("             " + lOrderItem.Book.Title + ": Quantity " + lOrderItem.Quantity);
                        }
                        
                        Console.WriteLine("=============================================" + "\n");
                        Console.WriteLine(" ");

                        // Restore stocks
                        RestoreStock(orderItems, lContainer);

                        // ask the Bank service to transfer fundss
                        result = TransferFundsToCustomer(UserProvider.ReadUserById(lOrder.Customer.Id).BankAccountNumber, lOrder.Total ?? 0.0);

                        // Delete the delivery in the delivery table 
                        DeleteDelivery(lOrder.OrderNumber.ToString());

                        lOrder.OrderItems.ToList().ForEach(o => { lContainer.Entry(o).State = System.Data.Entity.EntityState.Deleted; });
                        lContainer.Entry(lOrder).State = System.Data.Entity.EntityState.Deleted;
                        lContainer.Orders.Remove(lOrder);

                        // save the changes
                        lContainer.SaveChanges();
                        lScope.Complete();
                    }
                    catch (Exception lException)
                    {
                        Console.WriteLine("================Cancel Order=================");
                        Console.WriteLine("             Order ID: " + pOrderId);
                        Console.WriteLine("             CustomerID: " + lOrder.Customer.Id);
                        Console.WriteLine("             Name: " + lOrder.Customer.Name);
                        Console.WriteLine("             Address: " + lOrder.Customer.Address);
                        Console.WriteLine("             Time: " + DateTime.Now);
                        Console.WriteLine("Failed to restore the order items");
                        
                        Console.WriteLine("=============================================" + "\n");
                        Console.WriteLine(" ");

                        // need to rollback bank transfer if the transfer happened
                        if (result == "Transfer Success")
                        {
                            TransferFundsFromCustomer(UserProvider.ReadUserById(lOrder.Customer.Id).BankAccountNumber, lOrder.Total ?? 0.0);
                        }
                        SendOrderErrorMessage(lOrder, lException);
                        IEnumerable<System.Data.Entity.Infrastructure.DbEntityEntry> entries = lContainer.ChangeTracker.Entries();
                    }
                }
            }
            // we have a customer to send the cancelled confirmation to
            if (customerEmail != "" && orderNumber != Guid.Empty) SendOrderCancelledConfirmation(customerEmail, orderNumber);
        }

        private bool DeleteDelivery(string OrderNumber)
        {
            return ExternalServiceFactory.Instance.DeliveryService.DeleteDelivery(OrderNumber);
        }


        // Code taken from stack overflow
        // Gets all permutations of a list of given length
        // https://stackoverflow.com/questions/1952153/what-is-the-best-way-to-find-all-combinations-of-items-in-an-array
        static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(o => !t.Contains(o)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        // Loads the optimals warehouses and returns them
        private List<Warehouse> LoadOptimalWarehouseStocks(Order pOrder)
        {
            try
            {
                List<Warehouse> newList = new List<Warehouse>();

                if (!pOrder.canSatisfy()) return newList;

                using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
                {
                    // Get all warehouses
                    List<Warehouse> lWarehouses = lContainer.Warehouses.ToList<Warehouse>();

                    int length = 1;

                    // Brute force algorithm
                    Boolean satisfy = false;

                    int totalAmount = 0;

                    // Keep track of the total amount of stocks
                    foreach(OrderItem lOrderitem in pOrder.OrderItems)
                    {
                        totalAmount += lOrderitem.Quantity;
                    }

                    while (!satisfy)
                    {
                        // Get permutations
                        IEnumerable<IEnumerable<Warehouse>> lPermutedWarehouses = GetPermutations(lWarehouses, length);

                        // For each combination of warehouses
                        foreach (IEnumerable<Warehouse> warehouses in lPermutedWarehouses)
                        {
                            int tempTotal = totalAmount;
                            satisfy = true;

                            // For every order item
                            foreach(OrderItem lOrderItem in pOrder.OrderItems)
                            {
                                // Get the total amount of that item
                                int totalQuantity = 0;

                                totalQuantity += lOrderItem.Quantity;

                                // Check if all the warehouses can satisfy that item
                                foreach (Warehouse w in warehouses)
                                {
                                    foreach (Stock stock in w.Stocks)
                                    {
                                        // if it's the same book
                                        if (stock.Book.Id == lOrderItem.Book.Id)
                                        {
                                            if (stock.Quantity.Value >= lOrderItem.Quantity)
                                            {
                                                // Satisfy it
                                                totalQuantity = 0;
                                            }
                                            else
                                            {
                                                totalQuantity -= (stock.Quantity.Value);
                                            }
                                        }
                                    }
                                }

                                // Subtract the total amount of items from the total order amount
                                if(totalQuantity == 0)
                                {
                                    tempTotal -= lOrderItem.Quantity;
                                }
                            }
                            
                            if (tempTotal > 0)
                            {
                                satisfy = false;
                            }

                            // if every item can be satisfied
                            // Add it to the List 
                            if (satisfy)
                            {
                                Console.WriteLine("Best combination: ");
                                foreach(Warehouse w in warehouses)
                                {
                                    Console.WriteLine("Warehouse ID: " + w.Id);
                                    newList.Add(w);
                                }
                                return newList;
                            }
                        }

                        // Cannot satisfy the order in n warehouses, try n+1 warehouses
                        length++;
                    }
                    return newList;
                }
            }
            catch (InsufficientStockException e)
            {
                Console.WriteLine("Exception: No stock left");

                return new List<Warehouse>();
            }
        }

        private void SendOrderErrorMessage(Order pOrder, Exception pException)
        {
            try
            {
                EmailProvider.SendMessage(new EmailMessage()
                {
                    ToAddress = pOrder.Customer.Email,
                    Message = "There was an error in processsing your order " + pOrder.OrderNumber + ": " + pException.Message + ". Please contact Book Store"
                });
            }
            catch (Exception lException)
            {
                Console.WriteLine("=================Email====================");
                Console.WriteLine("From: BookStore");
                Console.WriteLine("To: " + pOrder.Customer.Email);
                Console.WriteLine("Order ID: " + pOrder.OrderNumber);
                Console.WriteLine("Transfer time: " + DateTime.Now);
                Console.WriteLine("Message: " + lException.Message);
                Console.WriteLine("Status: FAILED");
                Console.WriteLine("==========================================" + "\n");
                Console.WriteLine("Failed to send email to customer about error in processing order");
            }
        }

        private void SendOrderDeletedErrorMessage(string pCustomerEmail, Guid pOrderId)
        {
            try
            {
                EmailProvider.SendMessage(new EmailMessage()
                {
                    ToAddress = pCustomerEmail,
                    Message = "Just verifyng that your order of id " + pOrderId + " was cancelled and hence could not be delivered"
                });
            }
            catch (Exception lException)
            {
                Console.WriteLine("=================Email====================");
                Console.WriteLine("From: BookStore");
                Console.WriteLine("To: " + pCustomerEmail);
                Console.WriteLine("Order ID: " + pOrderId);
                Console.WriteLine("Transfer time: " + DateTime.Now);
                Console.WriteLine("Status: FAIED");
                Console.WriteLine("Message: " + lException.Message);
                Console.WriteLine("==========================================" + "\n");
                Console.WriteLine("Failed to send email to customer about error in processing order");
            }
        }

        private void SendOrderPlacedConfirmation(Order pOrder)
        {
            try
            {
                EmailProvider.SendMessage(new EmailMessage()
                {
                    ToAddress = pOrder.Customer.Email,
                    Message = "Your order " + pOrder.OrderNumber + " has been placed"
                });
            }
            catch (Exception lException)
            {
                Console.WriteLine("=================Email====================");
                Console.WriteLine("From: BookStore");
                Console.WriteLine("To: " + pOrder.Customer.Email);
                Console.WriteLine("Order ID: " + pOrder.OrderNumber);
                Console.WriteLine("Message " + lException.Message);
                Console.WriteLine("OrderConfirmation time: " + DateTime.Now);
                Console.WriteLine("Status: FAILURE");
                Console.WriteLine("==========================================" + "\n");
            }
        }

        private void SendOrderConfirmedConfirmation(Order pOrder)
        {
            try
            {
                EmailProvider.SendMessage(new EmailMessage()
                {
                    ToAddress = pOrder.Customer.Email,
                    Message = "Your order " + pOrder.OrderNumber + " has been accepted and processed!"
                });
            }
            catch (Exception lException)
            {
                Console.WriteLine("=================Failed Email====================");
                Console.WriteLine("From: BookStore");
                Console.WriteLine("To: " + pOrder.Customer.Email);
                Console.WriteLine("Order ID: " + pOrder.OrderNumber);
                Console.WriteLine("Message " + lException.Message);
                Console.WriteLine("OrderConfirmation time: " + DateTime.Now);
                Console.WriteLine("Status: FAILURE");
                Console.WriteLine("==========================================" + "\n");
            }
        }

        private void SendOrderCancelledConfirmation(String customerEmail, Guid orderEmail)
        {
            try
            {
                EmailProvider.SendMessage(new EmailMessage()
                {
                    ToAddress = customerEmail,
                    Message = "Your order " + orderEmail + " has been cancelled"
                });
            }
            catch (Exception lException)
            {
                Console.WriteLine("=================Failed Email====================");
                Console.WriteLine("From: BookStore");
                Console.WriteLine("To: " + customerEmail);
                Console.WriteLine("Order ID: " + orderEmail);
                Console.WriteLine("Cancellation time: " + DateTime.Now);
                Console.WriteLine("Status: Order Cancelled, Email not sent");
                Console.WriteLine("==========================================" + "\n");
                Console.WriteLine("Failed to send email to customer about order cancellation");
            }
        }

        private void GetDeliveryAddress(HashSet<String> pAddresses, Order pOrder)
        {
            foreach (OrderItem oi in pOrder.OrderItems)
            {
                foreach (OrderStock os in oi.OrderStocks)
                {
                    pAddresses.Add(os.Stock.Warehouse.Address.ToString());
                }
            }
        }

        private void PlaceDeliveryForOrder(Order pOrder, Delivery pDelivery)
        {
            Delivery lDelivery = pDelivery;
            OrderInfo lOrderInfo = new OrderInfo();

            foreach (OrderItem oi in pOrder.OrderItems)
            {
                String lBookTitle = oi.Book.Title;
                List<String> lWarehouses = new List<String>();

                foreach (OrderStock os in oi.OrderStocks)
                {
                    lWarehouses.Add(os.Stock.Warehouse.Name);
                }
                lOrderInfo.AddOrderItem(lBookTitle, lWarehouses);
            }

            DeliveryInfo lDeliveryInfo = new DeliveryInfo()
            {
                OrderNumber = lDelivery.Order.OrderNumber.ToString(),
                SourceAddress = lDelivery.SourceAddress,
                DestinationAddress = lDelivery.DestinationAddress,
                DeliveryNotificationAddress = "net.tcp://localhost:9010/DeliveryNotificationService"
            };

            Console.WriteLine("============Delivery Placed============");
            Console.WriteLine("SUBMITTING DELIVERY SERVICE with the");
            Console.WriteLine("following information:");
            Console.WriteLine("Order Number: " + lDelivery.Order.OrderDate.ToString());
            Console.WriteLine("Source Addr: " + lDelivery.SourceAddress);
            Console.WriteLine("Dest Addr: " + lDelivery.DestinationAddress);
            Console.WriteLine("Time: " + DateTime.Now);
            Console.WriteLine("=======================================");
            Console.WriteLine(" ");
            Guid lDeliveryIdentifier = ExternalServiceFactory.Instance.DeliveryService.SubmitDelivery(lDeliveryInfo, lOrderInfo);

            lDelivery.ExternalDeliveryIdentifier = lDeliveryIdentifier;
            pOrder.Delivery = lDelivery;
        }

        private string TransferFundsFromCustomer(int pCustomerAccountNumber, double pTotal)
        {
            try
            {
                Console.WriteLine("===========Calling BANK===========");
                Console.WriteLine("Intiating transfer:");
                Console.WriteLine("Account Number: " + pCustomerAccountNumber);
                Console.WriteLine("TOTAL: " + pTotal);
                Console.WriteLine("Time: " + DateTime.Now);
                Console.WriteLine("==================================");
                Console.WriteLine(" ");
                return ExternalServiceFactory.Instance.TransferService.Transfer(pTotal, pCustomerAccountNumber, RetrieveBookStoreAccountNumber());
            }
            catch
            {
                Console.WriteLine("===========Transferred Funds===========");
                Console.WriteLine("Time: " + DateTime.Now);
                Console.WriteLine("Status: FAIL");
                Console.WriteLine("Error in Acc Number or Total");
                Console.WriteLine("=======================================");
                Console.WriteLine(" ");
                throw new Exception("Error transferring funds to customer");
            }
        }

        private string TransferFundsToCustomer(int pCustomerAccountNumber, double pTotal)
        {
            try
            {
                Console.WriteLine("===========Calling BANK===========");
                Console.WriteLine("From: " + pCustomerAccountNumber);
                Console.WriteLine("Total: " + pTotal);
                Console.WriteLine("Time: " + DateTime.Now);
                Console.WriteLine("Status: SUCCESS");
                Console.WriteLine("=======================================");
                Console.WriteLine(" ");
                return ExternalServiceFactory.Instance.TransferService.Transfer(pTotal, RetrieveBookStoreAccountNumber(), pCustomerAccountNumber);
            } 
            catch
            {
                Console.WriteLine("===========Transferred Funds===========");
                Console.WriteLine("Time: " + DateTime.Now);
                Console.WriteLine("Status: FAIL");
                Console.WriteLine("Error in Acc Number or Total");
                Console.WriteLine("=======================================");
                Console.WriteLine(" ");
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

            List<OrderStock> orderStocks = (from OrderStock1 in pContainer.OrderStocks.Include("Stock").Include("OrderItem")
                                            where orderIds.Contains(OrderStock1.OrderItem.Id)
                                            select OrderStock1).ToList<OrderStock>();
            Console.WriteLine("================Stock Restore================");
            Console.WriteLine("Order items:");
            foreach (OrderStock orderStock in orderStocks)
            {
                Stock stock = pContainer.Stocks.SingleOrDefault(r => r.Id == orderStock.Stock.Id);

                // the item was not chosen
                if (stock == null) continue;

                Console.WriteLine("         " + orderStock.OrderItem.Book.Title + ": Quantity: " + orderStock.Quantity);

                stock.Quantity = stock.Quantity.Value + orderStock.Quantity;
                pContainer.Entry(orderStock).State = System.Data.Entity.EntityState.Deleted;
                pContainer.OrderStocks.Remove(orderStock);
                pContainer.Stocks.Attach(stock);
                pContainer.Entry(stock).Property(x => x.Quantity).IsModified = true;
                
            }
            Console.WriteLine("Time: " + DateTime.Now);
            Console.WriteLine("=============================================");
            Console.WriteLine(" ");
        }
    }
}
