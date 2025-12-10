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
        public async Task<IActionResult> Index()
        {
            string json = Request.Cookies["Basket"];
            List<BasketCookieItemVM> items;
            BasketVM basketVM = new()
            {
                BasketItemVMs = new List<BasketItemVM>()
            };
            if (json is not null)

            {
                items = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(json);
            }
            else
            {
                items = new();
            }
            foreach(BasketCookieItemVM cookie in items)
            {
              Product? product=await  _context.Products
                    .Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary==true))
                    .FirstOrDefaultAsync(p => cookie.ProductId == p.Id);
                if (product is not null)
                {
                    basketVM.BasketItemVMs.Add(new BasketItemVM
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        Image = product.ProductImages[0].Image,
                        Count = cookie.Count,
                        SubTotal = cookie.Count * product.Price
                    });
                    basketVM.Total += cookie.Count * product.Price;
                }

            }
            return View(basketVM);
        }
        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id is null || id < 1) return BadRequest();
            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            List<BasketCookieItemVM> items;
            if (Request.Cookies["Basket"] is null)
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
                string str = Request.Cookies["Basket"];
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
