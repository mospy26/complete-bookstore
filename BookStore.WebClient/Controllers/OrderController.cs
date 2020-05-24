using BookStore.WebClient.ViewModels;
using System;
using System.Threading;
using System.Web.Mvc;

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
            ThreadPool.QueueUserWorkItem(o => DeleteOrderRunnable(pOrderId));
            //return RedirectToAction("Index", new { pUserCache = pUserCache });
            return View("OrderConfirmation");
        }

        private void DeleteOrderRunnable(int pOrderId)
        {
            ServiceFactory.Instance.OrderService.CancelOrder(pOrderId);
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