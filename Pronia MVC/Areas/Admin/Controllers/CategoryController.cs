using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.ViewModels;


namespace Pronia_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;


        }
        public async Task<IActionResult> Index()
        {
            var categoriesVMs = await _context.Categories.Include(c => c.Products).Select(c => new GetCategoryVM
            {
                Id = c.Id,
                Name = c.Name,
                Products = c.Products.ToList()
            }).ToListAsync();
            return View(categoriesVMs);


        }
        public IActionResult Create()
        {
            CreateCategoryVM categoryVM = new()
            {

            };
            return View(categoryVM);

        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM categoryVM)
        {
            if (!ModelState.IsValid)
            {
                return View(categoryVM);
            }
            bool result = await _context.Categories.AnyAsync(c => c.Name == categoryVM.Name);
            if (result)
            {
                ModelState.AddModelError(nameof(CreateCategoryVM.Name), $"{categoryVM.Name} category already exist");
                return View(categoryVM);
            }
            Category category = new()
            {
                Name = categoryVM.Name,
                CreatedAt = DateTime.Now
            };


            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id < 1)
            {
                return BadRequest();
            }
            Category? existed = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (existed is null)
            {
                return NotFound();
            }
            UpdateCategoryVM categoryVM = new()
            {
                Name = existed.Name,
            };
            return View(categoryVM);

        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateCategoryVM categoryVM)
        {
            if (id == null || id < 1) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(categoryVM);
            }

            bool result = await _context.Categories.AnyAsync(c => c.Name == categoryVM.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdateCategoryVM.Name), $"{categoryVM.Name} category already exist");
                return View(categoryVM);
            }


            Category? existed = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (existed == null)
            {
                return NotFound();
            }

            existed.Name = categoryVM.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id is null || id < 1)
            {
                return BadRequest();
            }
            Category? existedCategory = await _context.Categories.Include(c=>c.Products).ThenInclude(p=>p.ProductImages).FirstOrDefaultAsync(c => c.Id == id);
            if (existedCategory is null)
            {
                return NotFound();
            }
            GetCategoryVM categoryVM = new()
            {
                Id = existedCategory.Id,
                Name = existedCategory.Name,
                Products = existedCategory.Products

            };
            return View(categoryVM);

        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null | id < 1)
            {
                return BadRequest();
            }
            Category? category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category is null)
            {
                return NotFound();
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
   
    
