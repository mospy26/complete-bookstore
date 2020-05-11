using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookStore.Business.Entities
{
    public partial class Order
    {
        public Boolean CheckStocks()
        {
            foreach (OrderItem lItem in this.OrderItems)
            {
                int lTotalQuantity = lItem.Quantity;
                int? lTotalStock = lItem.Book.Stocks.Sum(r => r.Quantity);

                // Not enough stock
                if (lTotalStock.Value < lTotalQuantity)
                {
                    return false;
                }
            }

            return true;
        }

        public List<Tuple<Stock, OrderItem, int>> UpdateStockLevels(Warehouse ware)
        {
            List<Tuple<Stock, OrderItem, int>> lStocks = new List<Tuple<Stock, OrderItem, int>>();
            // TODO Warehouse stuff
            foreach (OrderItem lItem in this.OrderItems)
            {
                int lTotalQuantity = lItem.Quantity;

                // Get the number of stocks of THAT book in THAT warehouse...
                int? lTotal = ware.Stocks.Select(stock => stock.Book.Id == lItem.Book.Id).ToList().Count();

                // It shouldnt be, but just in case...
                if(lTotal < lTotalQuantity)
                {
                    throw new Exception("Cannot place an order - This book is out of stock");
                }

                // IDEA: loop through every stock in the warehouse
                // If we find the stock = the order item then do the minusing etc...
                foreach(Stock lStock in ware.Stocks)
                {
                    // If same stock...
                    if(lStock.Book.Title == lItem.Book.Title)
                    {
                        if(lStock.Quantity < lItem.Quantity)
                        {
                            lStocks.Add(new Tuple<Stock, OrderItem, int>(lStock, lItem, lStock.Quantity.Value));
                            lItem.Quantity -= (int)lStock.Quantity;
                            lStock.Quantity = 0;
                        }
                        else
                        {
                            lStocks.Add(new Tuple<Stock, OrderItem, int>(lStock, lItem, lItem.Quantity));
                            lStock.Quantity -= lItem.Quantity;
                            lItem.Quantity = 0;
                        }
                    }
                }
                // ==================== old stuff =============================
                //int? lTotalStock = lItem.Book.Stocks.Sum(r => r.Quantity);

                //// Not enough stock
                //if (lTotalStock.Value < lTotalQuantity)
                //{
                //    throw new Exception("Cannot place an order - This book is out of stock");
                //}

                //// There is stock
                //foreach (Stock lStock in lItem.Book.Stocks.OrderByDescending(r => r.Quantity))
                //{
                //    if (lTotalQuantity == 0)
                //    {
                //        break;
                //    }
                //    if (lStock.Quantity - lTotalQuantity >= 0)
                //    {
                //        lStock.Quantity -= lTotalQuantity;
                //        lTotalQuantity = 0;
                //    }
                //    else if (lStock.Quantity < lTotalQuantity)
                //    {
                //        int? temp = lStock.Quantity;
                //        lStock.Quantity = 0;
                //        lTotalQuantity -= temp.Value;
                //    }
                //}
            }
            return lStocks;
        }
    }
}
