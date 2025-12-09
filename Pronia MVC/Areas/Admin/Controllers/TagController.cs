using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.ViewModels;

namespace Pronia_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TagController : Controller
    {
        private readonly AppDbContext _context;

        public TagController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var tagVMs = await _context.Tags.Select(c => new GetTagVM
            {
                Id = c.Id,
                Name = c.Name
            }).ToListAsync();
            return View(tagVMs);


        }
        public IActionResult Create()
        {
            CreateTagVM tagVM = new CreateTagVM();
            return View(tagVM);

        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateTagVM tagVM)
        {
            if (!ModelState.IsValid)
            {
                return View(tagVM);
            }
            bool result = await _context.Tags.AnyAsync(c => c.Name == tagVM.Name);
            if (result)
            {
                ModelState.AddModelError(nameof(CreateTagVM.Name), $"{tagVM.Name} tag already exist");
                return View(tagVM);
            }
            Tag tag = new()
            {
                Name = tagVM.Name,
                CreatedAt = DateTime.Now
            };


            _context.Add(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id < 1)
            {
                return BadRequest();
            }
            Tag? existed = await _context.Tags.FirstOrDefaultAsync(c => c.Id == id);
            if (existed is null)
            {
                return NotFound();
            }
            UpdateTagVM tagVM = new()
            {
                Name = existed.Name,
            };
            return View(tagVM);

        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateTagVM tagVM)
        {
            if (id is null || id < 1) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(tagVM);
            }

            bool result = await _context.Tags.AnyAsync(c => c.Name == tagVM.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdateTagVM.Name), $"{tagVM.Name} tag already exist");
                return View(tagVM);
            }


            Tag? existed = await _context.Tags.FirstOrDefaultAsync(c => c.Id == id);
            if (existed is null)
            {
                return NotFound();
            }

            existed.Name = tagVM.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null | id < 1)
            {
                return BadRequest();
            }
            Tag? tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag is null)
            {
                return NotFound();
            }
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null || id < 1)
            {
                return BadRequest();
            }

            Tag? existedTag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);

            if (existedTag is null)
            {
                return NotFound();
            }

            DetailsTagVM tagVM = new()
            {
                Name = existedTag.Name
            };

            return View(tagVM);
        }
    }

}
