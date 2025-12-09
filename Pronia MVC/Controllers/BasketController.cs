using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.ViewModels;

namespace Pronia_MVC.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;

        public BasketController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id is null || id < 1) return BadRequest();
            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            List<BasketCookieItemVM> items;
            if (Request.Cookies["basket"] is null)
            {
                items = new List<BasketCookieItemVM>();
                items.Add(new BasketCookieItemVM
                {
                    ProductId = id.Value,
                    Count = 1
                });
            }
            else
            {
                string str = Request.Cookies["basket"];
                items = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(str);
                BasketCookieItemVM itemVM = items.FirstOrDefault(i => i.ProductId == id);
                if (itemVM is null)
                {
                    itemVM = new BasketCookieItemVM
                    {
                        ProductId = id.Value,
                        Count = 1
                    };
                    items.Add(itemVM);
                }
                else
                {
                    itemVM.Count++;
                }


            }

            string json = JsonConvert.SerializeObject(items);
            Response.Cookies.Append("Basket", json);
            return RedirectToAction("Index", "Home");
        }
        public IActionResult GetBasket()
        {
            return Content(Request.Cookies["Basket"]);
        }
    }
}
