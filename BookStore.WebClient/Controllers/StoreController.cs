using BookStore.WebClient.ViewModels;
using System.Web.Mvc;

namespace BookStore.WebClient.Controllers
{
    public class StoreController : Controller
    {
        // GET: Store
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListBooks(int? pPageNumber)
        {
            pPageNumber = pPageNumber != null ? pPageNumber - 1 : 0;
            return View(new CatalogueViewModel(pPageNumber ?? 0));
        }
    }
}