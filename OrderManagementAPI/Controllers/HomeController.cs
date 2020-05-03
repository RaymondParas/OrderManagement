using System.Web.Mvc;

namespace OrderManagementAPI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Login()
        {
            ViewBag.Title = "Login Page";
            return View();
        }

        public ActionResult Dashboard()
        {
            ViewBag.Title = "Dashboard Page";
            return View();
        }
    }
}
