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
                Console.WriteLine(ise.ItemName);    
                throw new FaultException<InsufficientStockFault>(
                    new InsufficientStockFault() { ItemName = ise.ItemName }, "Insufficient Stock, cannot make order");
            }
        }

        public void CancelOrder(int pOrderId)
        {
            try
            {
                OrderProvider.CancelOrder(pOrderId);
            } catch (BookStore.Business.Entities.OrderDoesNotExistException e)
            {
                throw new FaultException<OrderDoesNotExistFault>(
                    new OrderDoesNotExistFault() { OrderId = e.OrderId }, "Order does not exist!");
            } catch (BookStore.Business.Entities.OrderHasAlreadyBeenDeliveredException e)
            {
                throw new FaultException<OrderHasAlreadyBeenDeliveredFault>(
                    new OrderHasAlreadyBeenDeliveredFault() { OrderId = e.OrderId }, "Your order has already been delivered!");
            }
        }
    }
}
