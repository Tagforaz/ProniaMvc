using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.ViewModels;

namespace Pronia_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ColorController : Controller
    {
        private readonly AppDbContext _context;

        public ColorController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<GetColorVM> colorVMs = await _context.Colors
                .Select(c => new GetColorVM
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
            return View(colorVMs);
        }
        [HttpGet]
        public IActionResult Create()
        {
            CreateColorVM colorVM = new CreateColorVM();
            return View(colorVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateColorVM colorVM)
        {
            if (!ModelState.IsValid)
            {
                return View(colorVM);
            }

            bool result = await _context.Colors.AnyAsync(c => c.Name == colorVM.Name);
            if (result)
            {
                ModelState.AddModelError(nameof(CreateColorVM.Name), $"Size {colorVM.Name} already exists.");
                return View(colorVM);
            }

            Color color = new Color()
            {
                Name = colorVM.Name
            };

            _context.Colors.Add(color);
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

            Color existedColor = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);

            if (existedColor is null)
            {
                return NotFound();
            }

            UpdateColorVM colorVM = new UpdateColorVM()
            {
                Name = existedColor.Name
            };

            return View(colorVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateColorVM colorVM)
        {
            if (id is null || id < 1)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(colorVM);
            }
            bool result = await _context.Colors.AnyAsync(c => c.Name == colorVM.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdateColorVM.Name), $"Size {colorVM.Name} already exists.");
                return View(colorVM);
            }

            Color? existedColor = await _context.Colors
                .FirstOrDefaultAsync(c => c.Id == id);

            existedColor.Name = colorVM.Name;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}