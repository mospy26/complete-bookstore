using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Services.Interfaces;
using BookStore.Business.Components.Interfaces;
using Microsoft.Practices.ServiceLocation;
using BookStore.Services.MessageTypes;

using System.ServiceModel;

namespace BookStore.Services
{
    public class OrderService : IOrderService
    {

        private IOrderProvider OrderProvider
        {
            get
            {
                return ServiceFactory.GetService<IOrderProvider>();
            }
        }

        public List<int> GetOrders(int pUserId)
        {
            List<int> lOrdersBefore = OrderProvider.GetOrders(pUserId);
            //List<BookStore.Services.MessageTypes.Order> lOrderAfter = MessageTypeConverter.Instance.Convert<
            //        List<BookStore.Business.Entities.Order>,
            //        List<BookStore.Services.MessageTypes.Order>
            //    >(lOrdersBefore);

            //Console.WriteLine(lOrderAfter);

            return lOrdersBefore;
            
        }

        public void SubmitOrder(Order pOrder)
        {
            try
            {
                OrderProvider.SubmitOrder(
                    MessageTypeConverter.Instance.Convert<
                    BookStore.Services.MessageTypes.Order,
                    BookStore.Business.Entities.Order>(pOrder)
                );
            }
            catch(BookStore.Business.Entities.InsufficientStockException ise)
            {
                throw new FaultException<InsufficientStockFault>(
                    new InsufficientStockFault() { ItemName = ise.ItemName });
            }
        }

        public void CancelOrder(Order pOrder)
        {
            try
            {
                OrderProvider.CancelOrder(
                    MessageTypeConverter.Instance.Convert<
                    BookStore.Services.MessageTypes.Order,
                    BookStore.Business.Entities.Order>(pOrder)
                );
                // Gonna need to throw a more specific error here 
            } catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }
    }
}
