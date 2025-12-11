using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.Services.Interfaces;
using Pronia_MVC.ViewModels;
using System.Security.Claims;

namespace Pronia_MVC.Services.Implementations
{
    public class LayoutServices : ILayoutService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _accessor;

        public LayoutServices(AppDbContext context,IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }


        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);

            return settings;
        }

        public async Task<BasketVM> GetBasketAsync()
        {

            BasketVM basketVM = new()
            {
                BasketItemVMs = new List<BasketItemVM>()
            };
            if (_accessor.HttpContext.User.Identity.IsAuthenticated)
            {
                basketVM.BasketItemVMs = await _context.BasketItems
                    .Where(bi => bi.AppUserId == _accessor.HttpContext.User
                    .FindFirstValue(ClaimTypes.NameIdentifier))
                    .Select(bi => new BasketItemVM
                    {
                        ProductId = bi.ProductId,
                        Count = bi.Count,
                        Name = bi.Product.Name,
                        Price = bi.Product.Price,
                        Image = bi.Product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                        SubTotal = bi.Count * bi.Product.Price
                    })
                    .ToListAsync();
                basketVM.BasketItemVMs.ForEach(b => basketVM.Total += b.SubTotal);
            }
            else
            {
                string json = _accessor.HttpContext.Request.Cookies["Basket"];
                List<BasketCookieItemVM> items;

                if (json is not null)

                {
                    items = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(json);
                }
                else
                {
                    items = new();
                }
                foreach (BasketCookieItemVM cookie in items)
                {
                    Product? product = await _context.Products
                          .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
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
            }
            return basketVM;
        }

    }
}
