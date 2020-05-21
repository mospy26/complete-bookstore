using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using BookStore.Services.MessageTypes;
using BookStore.WebClient.ViewModels;

namespace BookStore.WebClient.ClientModels
{
    public class Cart
    {
        private List<OrderItem> mOrderItems = new List<OrderItem>();
        public IList<OrderItem> OrderItems { get { return mOrderItems.AsReadOnly(); } }

        public void AddItem(Book pBook, int pQuantity)
        {
            var lItem = mOrderItems.FirstOrDefault(oi => oi.Book.Id == pBook.Id);
            if (lItem == null)
            {
                mOrderItems.Add(new OrderItem() { Book = pBook, Quantity = pQuantity });
            }
            else
            {
                lItem.Quantity += pQuantity;
            }
        }

        public double ComputeTotalValue()
        {
            return mOrderItems.Sum(oi => oi.Book.Price * oi.Quantity);
        }

        public void Clear()
        {
            mOrderItems.Clear();
        }

        public void SubmitOrderAndClearCart(UserCache pUserCache)
        {

            Order lOrder = new Order();
            lOrder.OrderDate = DateTime.Now;
            lOrder.Customer = pUserCache.Model;
            lOrder.Status = 0;
            foreach (OrderItem lItem in mOrderItems)
            {
                lOrder.OrderItems.Add(lItem);
            }
            lOrder.Total = Convert.ToDouble(ComputeTotalValue());

            ThreadPool.QueueUserWorkItem(o => SubmitOrderRunnable(lOrder));
            pUserCache.UpdateUserCache();
            Clear();
        }

        public void SubmitOrderRunnable(Order pOrder)
        {
            ServiceFactory.Instance.OrderService.SubmitOrder(pOrder);
        }

        public void RemoveLine(Book pBook)
        {
            mOrderItems.RemoveAll(oi => oi.Book.Id == pBook.Id);
        }
    }
}