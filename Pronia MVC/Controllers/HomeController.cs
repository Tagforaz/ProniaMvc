using Microsoft.AspNetCore.Mvc;

namespace Pronia_MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
