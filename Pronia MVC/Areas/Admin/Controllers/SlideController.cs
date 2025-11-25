using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.Utilities.Extensions;

namespace Pronia_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SlideController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            
        }
        public async Task<IActionResult> Index()
        {
            List<Slide> slides = await _context.Slides.AsNoTracking().ToListAsync();
            return View(slides);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Slide slide )
        {
            if (!slide.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(Slide.Photo), "File type is incorrect");
                return View();
            }
            if (slide.Photo.ValidateSize(Utilities.Enums.FileSize.MB,2))
            {
                ModelState.AddModelError(nameof(Slide.Photo), "File size is incorrect");
                return View();
            }
            //if (!ModelState.IsValid)
            //{
            //    return View();
            //}
            bool result = await _context.Slides.AnyAsync(s => s.Order == slide.Order);
            if (result)
            {
                ModelState.AddModelError(nameof(Slide.Order), $"{slide.Order} order already exist");
                return View();
            }
            string fileName = string.Concat(Guid.NewGuid(),slide.Photo.FileName);
            string path = Path.Combine(_env.WebRootPath, "assets","images","website-images",fileName);
            FileStream stream = new FileStream(path,FileMode.Create);
            await slide.Photo.CopyToAsync(stream);
            slide.Image = fileName;
            slide.CreatedAt = DateTime.Now;

            _context.Add(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Update(int? id)
        {
            if(id is null || id < 1)
            {
                return BadRequest();
            }
            Slide existed=await _context.Slides.FirstOrDefaultAsync(s=>s.Id==id);
            if(existed is null)
            {
                return NotFound();
            }
            return View(existed);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id,Slide slide)
        {
            if (!ModelState.IsValid)
            {
                return View(slide);
            }
            bool result = await _context.Slides.AnyAsync(s=>s.Order == slide.Order && s.Id!=id);
            if(result)
            {
                ModelState.AddModelError(nameof(Slide.Order), $"{slide.Order} order already exist");
                return View(slide);
            }
            Slide existed = await _context.Slides.FirstOrDefaultAsync( s=>s.Id==id);
            if(existed is null)
            {
                return NotFound();
            }
            existed.Title = slide.Title;
            existed.Description = slide.Description;    
            existed.SubTitle=slide.SubTitle;
            existed.Order = slide.Order;
            existed.Image = slide.Image;

            await _context.SaveChangesAsync();
              
            return RedirectToAction(nameof(Index));
        }

        }

}
