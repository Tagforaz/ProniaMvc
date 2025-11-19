using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.ViewModels;

namespace Pronia_MVC.Controllers
{
    public class ShopController : Controller
    {
    
        private readonly AppDbContext _context;
        public ShopController(AppDbContext context)
        {
            _context = context;
        }
        [Route("shop")]
        public IActionResult Index()
        {
            return View();
        }

        //NULL False True
        public IActionResult Details(int? id)
        {
            if(id is  null || id < 1)
            {
                return BadRequest();
            }
            Product? product = _context.Products
                .Include(p=>p.ProductImages.OrderByDescending(pi=>pi.IsPrimary))
                .Include(p=>p.Category)
                .FirstOrDefault(p => p.Id == id);


            if(product is null)
            {
              return NotFound();
            }

            List<Product> relatedProducts=_context.Products
                .Where(p=>p.CategoryId==product.CategoryId && p.Id!=id)
                .Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary!=null))
                .ToList();

            DetailsVM detailsVM = new()
            {
                Product = product,
                RelatedProducts = relatedProducts
            };
            return View(detailsVM);
        }
    }
}
