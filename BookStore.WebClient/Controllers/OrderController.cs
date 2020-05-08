using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStore.WebClient.ViewModels;
using BookStore.Services.MessageTypes;
using System.ServiceModel;

namespace BookStore.WebClient.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order

        public ActionResult Index(UserCache pUserCache)
        {
            ListOrderItemViewModel ListOrderItemViewModel = new ListOrderItemViewModel(pUserCache.Model);
            return View(ListOrderItemViewModel);
        }

        public ActionResult DeleteOrder(int pOrderId, String pReturnUrl, UserCache pUserCache)
        {
            try
            {
                ServiceFactory.Instance.OrderService.CancelOrder(pOrderId);
            }
            catch (FaultException<OrderHasAlreadyBeenDeliveredFault> e)
            {
                return RedirectToAction("OrderHasAlreadyBeenDelivered", new { pOrderId = e.Detail.OrderId });
            }
            catch (FaultException<OrderDoesNotExistFault> e)
            {
                return RedirectToAction("OrderDoesNotExist", new { pOrderId = e.Detail.OrderId });
            }

            return RedirectToAction("Index", new { pUserCache = pUserCache });
        }

        public ActionResult OrderHasAlreadyBeenDelivered(int pOrderId)
        {
            return View(new OrderHasAlreadyBeenDeliveredViewModel(pOrderId));
        }

        public ActionResult OrderDoesNotExist(int pOrderId)
        {
            return View(new OrderDoesNotExistViewModel(pOrderId));
        }

    }
}