using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStore.WebClient.ViewModels;

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
            catch
            {
                return RedirectToAction("ErrorPage");
            }

            return RedirectToAction("Index", new { pUserCache = pUserCache });
        }

        public ActionResult ErrorPage()
        {
            return View();
        }

    }
}