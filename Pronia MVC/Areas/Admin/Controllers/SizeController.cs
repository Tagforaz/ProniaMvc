using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.ViewModels;

namespace Pronia_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SizeController : Controller
    {
        private readonly AppDbContext _context;

        public SizeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<GetSizeVM> sizeVMs = await _context.Sizes
                .Select(c => new GetSizeVM
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
            return View(sizeVMs);
        }
        [HttpGet]
        public IActionResult Create()
        {
            CreateSizeVM sizeVM = new CreateSizeVM();
            return View(sizeVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateSizeVM sizeVM)
        {
            if (!ModelState.IsValid)
            {
                return View(sizeVM);
            }

            bool result = await _context.Sizes.AnyAsync(c => c.Name == sizeVM.Name);
            if (result)
            {
                ModelState.AddModelError(nameof(CreateSizeVM.Name), $"Size {sizeVM.Name} already exists.");
                return View(sizeVM);
            }

            Size size = new Size()
            {
                Name = sizeVM.Name
            };

            _context.Sizes.Add(size);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id < 1)
            {
                return BadRequest();
            }

            Size existedSize = await _context.Sizes.FirstOrDefaultAsync(c => c.Id == id);

            if (existedSize is null)
            {
                return NotFound();
            }

            UpdateSizeVM sizeVM = new UpdateSizeVM()
            {
                Name = existedSize.Name
            };

            return View(sizeVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateSizeVM sizeVM)
        {
            if (id is null || id < 1)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(sizeVM);
            }
            bool result = await _context.Sizes.AnyAsync(c => c.Name == sizeVM.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdateSizeVM.Name), $"Size {sizeVM.Name} already exists.");
                return View(sizeVM);
            }

            Size? existedSize = await _context.Sizes
                .FirstOrDefaultAsync(c => c.Id == id);

            existedSize.Name = sizeVM.Name;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}