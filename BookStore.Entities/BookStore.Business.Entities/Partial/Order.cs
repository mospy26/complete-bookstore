using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookStore.Business.Entities
{
    public partial class Order
    {
        public List<Tuple<Stock, OrderItem, int>> UpdateStockLevels()
        {
            List<Tuple<Stock, OrderItem, int>> lConsumedStocks = new List<Tuple<Stock, OrderItem, int>>();
            
            // TODO Warehouse stuff

            foreach (OrderItem lItem in this.OrderItems)
            {
                int lTotalQuantity = lItem.Quantity;
                int? lTotalStock = lItem.Book.Stocks.Sum(r => r.Quantity);

                // Not enough stock
                if (lTotalStock.Value < lTotalQuantity)
                {
                    throw new InsufficientStockException() { ItemName = lItem.Book.Title };
                }

                // There is stock
                foreach (Stock lStock in lItem.Book.Stocks.OrderByDescending(r => r.Quantity))
                {
                    if (lTotalQuantity == 0)
                    {
                        break;
                    }
                    if (lStock.Quantity - lTotalQuantity >= 0)
                    {
                        lConsumedStocks.Add(new Tuple<Stock, OrderItem, int>(lStock, lItem, lTotalQuantity));
                        lStock.Quantity -= lTotalQuantity;
                        lTotalQuantity = 0;
                    }
                    else if (lStock.Quantity < lTotalQuantity)
                    {
                        lConsumedStocks.Add(new Tuple<Stock, OrderItem, int>(lStock, lItem, lStock.Quantity.Value));
                        int? temp = lStock.Quantity;
                        lStock.Quantity = 0;
                        lTotalQuantity -= temp.Value;
                    }
                }
            }
            return lConsumedStocks;
        }
    }
}
