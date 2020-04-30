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
                int? amountOfItemToDeduct = lItem.Quantity;

                foreach (Stock stock in lItem.Book.Stocks.OrderByDescending(r => r.Quantity))
                {
                    if (amountOfItemToDeduct == 0)
                    {
                        break;
                    } else if (amountOfItemToDeduct < stock.Quantity) 
                    {
                        amountOfItemToDeduct = 0;
                        stock.Quantity -= amountOfItemToDeduct;
                        break;
                    } else
                    {
                        amountOfItemToDeduct -= stock.Quantity;
                        stock.Quantity = 0;
                    }
                }

                if (amountOfItemToDeduct != 0)
                {
                    throw new Exception("Cannot place an order - This book is out of stock");
                }
            }

            //foreach (OrderItem lItem in this.OrderItems)
            //{
            //    if (lItem.Book.Stock.Quantity - lItem.Quantity >= 0)
            //    {
            //        lItem.Book.Stock.Quantity -= lItem.Quantity;
            //    }
            //    else
            //    {
            //        throw new Exception("Cannot place an order - This book is out of stock");
            //    }
            //}
        }
    }
}
