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
            return View(new ListOrderItemViewModel(pUserCache.Model));
        }
    }
}