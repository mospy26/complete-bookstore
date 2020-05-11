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

                        // Check if there is enough stock for the order
                        var check = pOrder.CheckStocks();

                        // If there is not enough stock throw new exception
                        if (!check)
                        {
                            throw new Exception("Cannot place an order - This book is out of stock");
                        }

                        // Else get the optimal warehouse
                        LoadOptimalWarehouseStocks(pOrder);
                        
                        // and update the stock levels
                        //pOrder.UpdateStockLevels();

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


        // Greedy algorithm...
        // NOT OPTIMAL
        // But finds a decent solution
        private void LoadOptimalWarehouseStocks(Order pOrder)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                // Warehosue and their total stocks
                List<Tuple<Warehouse, int>> lWarehouses = new List<Tuple<Warehouse, int>>();
                foreach (Warehouse w in lContainer.Warehouses)
                {
                    lWarehouses.Add(new Tuple<Warehouse, int>(w, w.Stocks.Select(x=> x.Quantity).Sum().Value));
                }
                lWarehouses.Sort((x, y) => y.Item2.CompareTo(x.Item2));

                // Temp variable to find the best warehouse (non optimal way as it uses greedy alg)
                var bestWarehouse = 0;

                bool satisfy = false;

                while (satisfy)
                {
                    Warehouse warehouseUsed = new Warehouse();

                    foreach (Tuple<Warehouse, int> w in lWarehouses)
                    {
                        Warehouse ware = w.Item1;

                        // Variable to hold how many products the current warehouse can satisfy
                        var currentBest = 0;

                        // Loops through items A B C...
                        foreach (OrderItem lOrderItem in pOrder.OrderItems)
                        {
                            // Variable holding amount of stock
                            var amount = 0;

                            // Check the stock of that warehouse to see if it can satisfy each other item...
                            // Get the amount of stock of a particular order item that is stored in that warehouse
                            var tempStock = ware.Stocks.Select(stock => stock.Book.Id == lOrderItem.Book.Id);
                            //var tempStock = lContainer.Stocks.Where((pStock) => pStock.Book.Id == lOrderItem.Book.Id && pStock.Warehouse.Id == ware.Id).ToList();

                            if (tempStock.Count() > lOrderItem.Quantity)
                            {
                                amount += lOrderItem.Quantity;
                            }
                            else
                            {
                                amount += tempStock.Count();
                            }

                            currentBest += amount;
                        }

                        // By this point currentBest will be the total amount of book the one warehouse can satisfy

                        if (currentBest > bestWarehouse)
                        {
                            bestWarehouse = currentBest;
                            warehouseUsed = ware;
                        }
                    }

                    // After you found the best warehouse...
                    // Compare quantities and reduce it

                    // Can we just do the submit order stuff here???

                    // NEW IDEA: pass in the warehouse as a parameter into update stock
                    // AND do this shit in there...
                    pOrder.UpdateStockLevels(warehouseUsed);

                    // Check if all order items are 0...
                    satisfy = true;
                    foreach(OrderItem OItem in pOrder.OrderItems)
                    {
                        if(OItem.Quantity > 0)
                        {
                            satisfy = false;
                        }
                    }
                }
                //foreach (OrderItem lOrderItem in pOrder.OrderItems)
                //{
                //    lOrderItem.Book.Stocks = lContainer.Stocks.Where((pStock) => pStock.Book.Id == lOrderItem.Book.Id).ToList<Stock>();    
                //}
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


        private int RetrieveBookStoreAccountNumber()
        {
            return 123;
        }


    }
}
