using Microsoft.AspNetCore.Mvc;

namespace Pronia_MVC.Controllers
{
    public class ShopController : Controller
    {
        [Route("shop")]
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details()
        {
            return View();
        }
    }
}
