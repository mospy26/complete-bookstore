using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookStore.Business.Entities
{
    public partial class Order
    {
        public Boolean canSatisfy()
        {
            foreach (OrderItem lItem in this.OrderItems)
            {
                int lTotalQuantity = lItem.Quantity;
                int? lTotalStock = lItem.Book.Stocks.Sum(r => r.Quantity);

                // Not enough stock
                if (lTotalStock.Value < lTotalQuantity)
                {
                    Console.WriteLine("================Order details================");
                    Console.WriteLine("             Order ID: " + this.Id);
                    Console.WriteLine("             CustomerID: " + this.Customer.Id);
                    Console.WriteLine("             Name: " + this.Customer.Name);
                    Console.WriteLine("             Address: " + this.Customer.Address);
                    Console.WriteLine("             Time: " + DateTime.Now);
                    Console.WriteLine("The order of " + lTotalQuantity + " " + lItem.Book.Title + " cannot be completed" + "\n");



                    Console.WriteLine("Reason: there is insufficient stock in all of the warehouses" + "\n");
                    Console.WriteLine("=============================================" + "\n");
                    throw new InsufficientStockException() { ItemName = lItem.Book.Title };
                }
            }
            return true;
        }

        public List<Tuple<Stock, OrderItem, int>> UpdateStockLevels(List<Warehouse> wares, BookStoreEntityModelContainer pContainer)
        {
            List<Tuple<Stock, OrderItem, int>> lConsumedStocks = new List<Tuple<Stock, OrderItem, int>>();

            // TODO Warehouse stuff

            // SHOULD ALWAYS WORK due to the optimal warehouse function
            foreach (OrderItem lItem in this.OrderItems)
            {
                int lTotalQuantity = lItem.Quantity;

                foreach (Warehouse w in wares)
                {
                    foreach (Stock s in lItem.Book.Stocks.Where(stock => stock.Warehouse.Id == w.Id))
                    {
                        if (lTotalQuantity == 0 || s.Quantity == 0)
                        {
                            break;
                        }

                        if (s.Quantity >= lTotalQuantity)
                        {
                            //pContainer.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            lConsumedStocks.Add(new Tuple<Stock, OrderItem, int>(s, lItem, lItem.Quantity));
                            Console.WriteLine("================Order details================");
                            Console.WriteLine("             Order ID: " + this.Id);
                            Console.WriteLine("             CustomerID: " + this.Customer.Id);
                            Console.WriteLine("             Name: " + this.Customer.Name);
                            Console.WriteLine("             Address: " + this.Customer.Address);
                            Console.WriteLine("             Time: " + DateTime.Now);
                            Console.WriteLine("Ordered: " + lItem.Quantity + " of " + lItem.Book.Title + " from Warehouse #" + w.Id);

                            Console.WriteLine("=============================================" + "\n");
                            //lItem.Quantity = 0;
                            s.Quantity -= lTotalQuantity;
                            lTotalQuantity = 0;
                        }
                        else
                        {
                            //pContainer.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            // case: <=
                            lConsumedStocks.Add(new Tuple<Stock, OrderItem, int>(s, lItem, s.Quantity.Value));
                            Console.WriteLine("================Order details================");
                            Console.WriteLine("             Order ID: " + this.Id);
                            Console.WriteLine("             CustomerID: " + this.Customer.Id);
                            Console.WriteLine("             Name: " + this.Customer.Name);
                            Console.WriteLine("             Address: " + this.Customer.Address);
                            Console.WriteLine("             Time: " + DateTime.Now);
                            Console.WriteLine("Ordered: " + s.Quantity.Value + " of " + lItem.Book.Title + " from Warehouse #" + w.Id);

                            Console.WriteLine("=============================================" + "\n");
                            //lItem.Quantity -= s.Quantity.Value;
                            lTotalQuantity -= s.Quantity.Value;
                            s.Quantity = 0;
                        }
                    }
                    if (lTotalQuantity == 0)
                    {
                        break;
                    }
                }
            }
            return lConsumedStocks;
        }
    }
}
