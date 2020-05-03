using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookStore.Business.Entities
{
    public partial class Order
    {
        public void UpdateStockLevels()
        {

            // TODO Warehouse stuff

            foreach (OrderItem lItem in this.OrderItems)
            {
                int lTotalQuantity = lItem.Quantity;
                int? lTotalStock = lItem.Book.Stocks.Sum(r => r.Quantity);

                // Not enough stock
                if (lTotalStock.Value < lTotalQuantity)
                {
                    throw new Exception("Cannot place an order - This book is out of stock");
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
                        lStock.Quantity -= lTotalQuantity;
                        lTotalQuantity = 0;
                    }
                    else if (lStock.Quantity < lTotalQuantity)
                    {
                        int? temp = lStock.Quantity;
                        lStock.Quantity = 0;
                        lTotalQuantity -= temp.Value;
                    }
                }
            }

            // Add all this to the new OrderStock table
        }
    }
}
