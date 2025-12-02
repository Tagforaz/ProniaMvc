using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.Utilities.Extensions;
using Pronia_MVC.ViewModels;


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
            List<GetSlideVM> slideVMs= await _context.Slides.AsNoTracking()
                .Select(s=>new GetSlideVM
                {
                 Id = s.Id,
                 Title = s.Title,
                 Image = s.Image,
                 Order = s.Order
                }).ToListAsync();
            return View(slideVMs);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateSlideVM slideVM )
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (!slideVM.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateSlideVM.Photo), "File type is incorrect");
                return View();
            }
            if (slideVM.Photo.ValidateSize(Utilities.Enums.FileSize.KB,2))
            {
                ModelState.AddModelError(nameof(CreateSlideVM.Photo), "File size is incorrect");
                return View();
            }
          
            bool result = await _context.Slides.AnyAsync(s => s.Order == slideVM.Order);
            if (result)
            {
                ModelState.AddModelError(nameof(Slide.Order), $"{slideVM.Order} order already exist");
                return View();
            }
            string fileName = await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");

            Slide slide = new Slide
            {
                Title = slideVM.Title,
                SubTitle = slideVM.SubTitle,
                Order = slideVM.Order,
                Description = slideVM.Description,
                Image = fileName,
                CreatedAt = DateTime.Now,
                IsDeleted=false
            };
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
            UpdateSlideVM slideVM = new UpdateSlideVM
            {
                Title = existed.Title,
                SubTitle = existed.SubTitle,
                Order = existed.Order,
                Description = existed.Description,
                Image = existed.Image

            };
            return View(slideVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id,UpdateSlideVM slideVM)
        {
            if (!ModelState.IsValid)
            {
                return View(slideVM);
            }
            if(slideVM.Photo is not  null)
            {
                if (!slideVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "File type is incorrect");
                    return View(slideVM);
                }
                if (!slideVM.Photo.ValidateSize(Utilities.Enums.FileSize.KB,2))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "File size is incorrect");
                    return View(slideVM);
                }
               
            }

            bool result = await _context.Slides.AnyAsync(s=>s.Order == slideVM.Order && s.Id!=id);
            if(result)
            {
                ModelState.AddModelError(nameof(UpdateSlideVM.Order), $"{slideVM.Order} order already exist");
                return View(slideVM);
            }
            Slide existed = await _context.Slides.FirstOrDefaultAsync( s=>s.Id==id);
            if(existed is null)
            {
                return NotFound();
            }

            if(slideVM.Photo is not null)
            {
                string fileName = await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                existed.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.Image = fileName;
            }

            existed.Title = slideVM.Title;
            existed.Description = slideVM.Description;    
            existed.SubTitle=slideVM.SubTitle;
            existed.Order = slideVM.Order;
           

            await _context.SaveChangesAsync();
              
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int?  id)
        {
            if (id is null || id < 1)
            {
                return BadRequest();
            }
            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null)
            {
                return NotFound();
            }
            existed.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
            _context.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null || id < 1)
            {
                return BadRequest();
            }
            Slide existedSlide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existedSlide is null)
            {
                return NotFound();
            }
            DetailsSlideVM slideVM = new()
            {
                Title=existedSlide.Title,
                SubTitle=existedSlide.SubTitle,
                Order=existedSlide.Order,
                Description=existedSlide.Description,
                Image=existedSlide.Image
             

            };
            return View(slideVM);
        }
    }

}
