using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.ViewModels;

namespace Pronia_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var productsVMs=await _context.Products
                .Select(p=>new GetAdminProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Image=p.ProductImages.FirstOrDefault(pi=>pi.IsPrimary==true).Image,
                    CategoryName=p.Category.Name
                }).ToListAsync();
            return View(productsVMs);
        }
        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new()
            {
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                Colors = await _context.Colors.ToListAsync(),
                Sizes = await _context.Sizes.ToListAsync(),
            };
            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories= await _context.Categories.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();
            productVM.Colors = await _context.Colors.ToListAsync();
            productVM.Sizes = await _context.Sizes.ToListAsync();
            if (!ModelState.IsValid)
            {

                return View(productVM);
            }
            bool result = productVM.Categories.Any(c=>c.Id==productVM.CategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(CreateProductVM.CategoryId), "Category does not exists(Men yazmisam)");
                return View(productVM);
            }
          if(productVM.TagIds is   null)
            {
                productVM.TagIds = new();
            }
          productVM.TagIds=productVM.TagIds.Distinct().ToList();
                bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));
                if (tagResult)
                {
                    ModelState.AddModelError(nameof(CreateProductVM.TagIds), "Tags are wrong");
                    return View(productVM);
                }
            if (productVM.ColorIds is  null)
            {
                productVM.ColorIds = new();
            }
            productVM.ColorIds = productVM.ColorIds.Distinct().ToList();
            bool colorResult = productVM.ColorIds.Any(cId => !productVM.Colors.Exists(cr =>cr.Id==cId));
            if (colorResult)
            {
                ModelState.AddModelError(nameof(CreateProductVM.ColorIds), "Colors are wrong");
                return View(productVM);
            }

            if (productVM.SizeIds is null)
            {
                productVM.SizeIds = new();
            }
            productVM.SizeIds = productVM.SizeIds.Distinct().ToList();
            bool sizeResult = productVM.SizeIds.Any(sId => !productVM.Sizes.Exists(sz => sz.Id == sId));
            if (sizeResult)
            {
                ModelState.AddModelError(nameof(CreateProductVM.SizeIds), "Sizes are wrong");
                return View(productVM);
            }
            bool resultName = await _context.Products.AnyAsync(p=>p.Name==productVM.Name);
            if (resultName)
            {
                ModelState.AddModelError(nameof(CreateProductVM.Name), "Product name already exists");
                return View(productVM);
            }
            Product product = new()
            {
                Name=productVM.Name,
                Sku=productVM.Sku,
                Price=productVM.Price.Value,
                Description=productVM.Description,
                CategoryId=productVM.CategoryId.Value,
                CreatedAt = DateTime.Now,
                ProductTags=productVM.TagIds.Select(tId=>new ProductTag { TagId=tId }).ToList(),
                ProductColors=productVM.ColorIds.Select(cId=> new ProductColor { ColorId=cId }).ToList(),
                ProductSizes = productVM.SizeIds.Select(sId => new ProductSize { SizeId =sId }).ToList()
            };

            //foreach (var tId  in productVM.TagIds)
            //{
            //    product.ProductTags.Add(new ProductTag
            //    {
            //        TagId = tId

            //    });
            //}
            
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if(id is null || id < 1)
            {
                return BadRequest();
            }
            Product? existed = await _context.Products
                .Include(p=>p.ProductTags)
                .Include(p=>p.ProductColors)
                .Include(p=>p.ProductSizes)
                .FirstOrDefaultAsync(p => p.Id==id); 
            if(existed is null)
            {
                return NotFound();
            }
            UpdateProductVM productVM = new()
            {
                Name=existed.Name,
                Sku=existed.Sku,
                Description=existed.Description,
                CategoryId=existed.CategoryId,
                Price=existed.Price,
                TagIds=existed.ProductTags.Select(pt=>pt.TagId).ToList(),
                ColorIds = existed.ProductColors.Select(pc => pc.ColorId).ToList(),   
                SizeIds = existed.ProductSizes.Select(ps => ps.SizeId).ToList(),
                Categories = await _context.Categories.ToListAsync(),
                Tags=await _context.Tags.ToListAsync(),
                Colors = await _context.Colors.ToListAsync(),
                Sizes = await _context.Sizes.ToListAsync()
            };
            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id,UpdateProductVM productVM)
        {
            productVM.Categories= await _context.Categories.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();
            productVM.Colors = await _context.Colors.ToListAsync();
            productVM.Sizes = await _context.Sizes.ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(productVM);
            }
            bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(UpdateProductVM.CategoryId), "Category does not exists(Men yazmisam)");
                return View(productVM);
            }
            if (productVM.TagIds is  null)
            {
                productVM.TagIds = new();
            }
            productVM.TagIds = productVM.TagIds.Distinct().ToList();
            bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));
            if (tagResult)
            {
                ModelState.AddModelError(nameof(UpdateProductVM.TagIds), "Tags are wrong");
                return View(productVM);
            }
            
            if (productVM.ColorIds is null)
            {
                productVM.ColorIds = new();
            }
            productVM.ColorIds = productVM.ColorIds.Distinct().ToList();
            bool colorResult = productVM.ColorIds.Any(cId => !productVM.Colors.Exists(cr => cr.Id == cId));
            if (colorResult)
            {
                ModelState.AddModelError(nameof(UpdateProductVM.ColorIds), "Colors are wrong");
                return View(productVM);
            }
            
            if (productVM.SizeIds is null)
            {
                productVM.SizeIds = new();
            }
            productVM.SizeIds = productVM.SizeIds.Distinct().ToList();
            bool sizeResult = productVM.SizeIds.Any(sId => !productVM.Sizes.Exists(sz => sz.Id == sId));
            if (sizeResult)
            {
                ModelState.AddModelError(nameof(UpdateProductVM.SizeIds), "Sizes are wrong");
                return View(productVM);
            }
            bool resultName = await _context.Products.AnyAsync(p => p.Name == productVM.Name&& p.Id!=id);
            if (resultName)
            {
                ModelState.AddModelError(nameof(UpdateProductVM.Name), "Product name already exists");
                return View(productVM);
            }
            Product? existed = await _context.Products
                .Include(p=>p.ProductTags)
                .Include(p => p.ProductColors)
                .Include(p => p.ProductSizes)
                .FirstOrDefaultAsync(p=> p.Id==id);


            _context.ProductTags
                .RemoveRange(existed.ProductTags
                .Where(pt => !productVM.TagIds
                .Contains(pt.TagId))
                .ToList());

            _context.ProductTags
                .AddRange(productVM.TagIds
         .Where(tId => !existed.ProductTags
         .Any(pt => pt.TagId == tId))
         .Select(tId => new ProductTag { TagId = tId, ProductId = existed.Id }));

            _context.ProductSizes
                .RemoveRange(existed.ProductSizes
                .Where(ps => !productVM.SizeIds
                .Contains(ps.SizeId))
                .ToList());

            _context.ProductSizes
                .AddRange(productVM.SizeIds
                    .Where(sId => !existed.ProductSizes
                    .Any(ps => ps.SizeId == sId))
                    .Select(sId => new ProductSize { SizeId = sId, ProductId = existed.Id }));

            _context.ProductColors
                .RemoveRange(existed.ProductColors
                .Where(pc => !productVM.ColorIds
                .Contains(pc.ColorId))
                .ToList());

            _context.ProductColors
                .AddRange(productVM.ColorIds
                    .Where(cId => !existed.ProductColors
                    .Any(pc => pc.ColorId == cId))
                    .Select(cId => new ProductColor { ColorId = cId, ProductId = existed.Id }));
            existed.Name = productVM.Name;
            existed.Sku = productVM.Sku;
            existed.Description = productVM.Description;
            existed.Price= productVM.Price.Value;
            existed.CategoryId=productVM.CategoryId.Value;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
