using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.ViewModels;
using System.Reflection;

namespace Pronia_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult>  Index()
        {
            //using AppDbContext context = new AppDbContext();
            //_context.Slides.ToList();

           

            HomeVm homeVm = new HomeVm
            {
                Slides = await _context.Slides
                .OrderBy(s => s.Order)
                .Take(2)
                .ToListAsync(),

                Products = await _context.Products
                .OrderBy(p => p.CreatedAt)
                .Take(8)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .ToListAsync()
                
            };
            return View(homeVm);

            //List<Slide> slides = new List<Slide>()
            //{
            //    new Slide
            //    {

            //        Title="Bashliq 1",
            //        SubTitle="Komekci Bashliq 1",
            //        Description="Gul 1",
            //        CreatedAt=DateTime.Now,
            //        Image="1-1-524x617.png",
            //        IsDeleted=false,
            //        Order=2
            //    },
            //    new Slide
            //    {
            //        Title="Bashliq 2",
            //        SubTitle="Komekci Bashliq 2",
            //        Description="Gul 2",
            //        CreatedAt=DateTime.Now,
            //        Image="flower.jpg",
            //        IsDeleted=false,
            //        Order =3
            //    },
            //    new Slide
            //    {
            //        Title="Bashliq 3",
            //        SubTitle="Komekci Bashliq 3",
            //        Description="Gul 3",
            //        CreatedAt=DateTime.Now,
            //        Image="1-2-524x617.png",
            //        IsDeleted=false,
            //        Order =1
            //    }
            //};
            //_context.Slides.AddRange(slides);
            //_context.SaveChanges();
           
        }
         
    }
}
