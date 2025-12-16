using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.Utilities.Enums;
using Pronia_MVC.Utilities.Extensions;
using Pronia_MVC.ViewModels;

namespace Pronia_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
       
        public async Task<IActionResult> Index(int page=1)
        {
            int totalCount =await _context.Products.CountAsync();
            int total = (int)Math.Ceiling((double)totalCount / 3);
            if(page<1 || page>total)
            {

                return BadRequest();
            }
            var productsVMs=await _context.Products
                .Skip((page-1)*3)
                .Take(3)
                .Select(p=>new GetAdminProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Image=p.ProductImages.FirstOrDefault(pi=>pi.IsPrimary==true).Image,
                    CategoryName=p.Category.Name,
                    CategoryId=p.Category.Id
                }).ToListAsync();
            PaginatedItemsVM<GetAdminProductVM> paginatedVM = new()
            {
                Items = productsVMs,
                CurrentPage = page,
                TotalPage = total
            };
            return View(paginatedVM);
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
            if (!productVM.PrimaryPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateProductVM.PrimaryPhoto), "File type is incorrect");
                return View(productVM);
            }
            if (!productVM.PrimaryPhoto.ValidateSize(FileSize.MB,1))
            {
                ModelState.AddModelError(nameof(CreateProductVM.PrimaryPhoto), "File size is incorrect");
                return View(productVM);
            }
            if (!productVM.SecondaryPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateProductVM.SecondaryPhoto), "File type is incorrect");
                return View(productVM);
            }
            if (!productVM.SecondaryPhoto.ValidateSize(FileSize.MB, 1))
            {
                ModelState.AddModelError(nameof(CreateProductVM.SecondaryPhoto), "File size is incorrect");
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

            ProductImage main = new ProductImage
            {
                Image = await productVM.PrimaryPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                IsPrimary = true,
                CreatedAt = DateTime.Now
            };
            ProductImage secondary = new ProductImage
            {
                Image = await productVM.SecondaryPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                IsPrimary = false,
                CreatedAt = DateTime.Now
            };
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
                ProductSizes = productVM.SizeIds.Select(sId => new ProductSize { SizeId =sId }).ToList(),
                ProductImages = new() { main, secondary }
            };
            if(productVM.AdditionalPhotos is not null)
            {
                string message = string.Empty;
                foreach (IFormFile file in productVM.AdditionalPhotos)
                {
                    if (!file.ValidateType("image/"))
                    {

                        message += $"<p class=\"text-warning\">{file.FileName} file type is incorrect</p>";
                        continue;
                    }
                    if (!file.ValidateSize(FileSize.MB, 1))
                    {
                        message += $"<p class=\"text-warning\">{file.FileName} file size is incorrect</p>"; ;
                        continue;
                    }

                    product.ProductImages.Add(new()
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                        IsPrimary = null,
                        CreatedAt = DateTime.Now
                    }
                    );
                }
                TempData["ImageWarning"] = message;
            }
            
  
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if(id is null || id < 1)
            {
                return BadRequest();
            }
            Product? existed = await _context.Products
                .Include(p => p.ProductImages)
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
                Sizes = await _context.Sizes.ToListAsync(),
                ProductImages = existed.ProductImages
            };
            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id,UpdateProductVM productVM)
        {
            Product? existed = await _context.Products
               .Include(p => p.ProductImages)
               .Include(p => p.ProductTags)
               .Include(p => p.ProductColors)
               .Include(p => p.ProductSizes)
               .FirstOrDefaultAsync(p => p.Id == id);
            productVM.Categories= await _context.Categories.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();
            productVM.Colors = await _context.Colors.ToListAsync();
            productVM.Sizes = await _context.Sizes.ToListAsync();
            productVM.ProductImages=existed.ProductImages;
           
            if (!ModelState.IsValid)
            {
                return View(productVM);
            }
            if(productVM.PrimaryPhoto is not null)
            {
                if (!productVM.PrimaryPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.PrimaryPhoto), "File type is incorrect");
                    return View(productVM);
                }
                if (!productVM.PrimaryPhoto.ValidateSize(FileSize.MB, 1))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.PrimaryPhoto), "File size is incorrect");
                    return View(productVM);
                }
            }
            if (productVM.SecondaryPhoto is not null)
            {
                if (!productVM.SecondaryPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.SecondaryPhoto), "File type is incorrect");
                    return View(productVM);
                }
                if (!productVM.SecondaryPhoto.ValidateSize(FileSize.MB, 1))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.SecondaryPhoto), "File size is incorrect");
                    return View(productVM);
                }
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
            
            if(productVM.PrimaryPhoto is not null)
            {
                string mainFileName = await productVM.PrimaryPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage existedMain =existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true);
                existedMain.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(existedMain);
                existed.ProductImages.Add(new()
                {
                    Image = mainFileName,
                    IsPrimary = true,
                    CreatedAt = DateTime.Now
                });
            }
            if (productVM.SecondaryPhoto is not null)
            {
                string secondaryName = await productVM.SecondaryPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage existedSecondary = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false);
                existedSecondary.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(existedSecondary);
                existed.ProductImages.Add(new()
                {
                    Image = secondaryName,
                    IsPrimary = false,
                    CreatedAt = DateTime.Now
                });
            }
            if(productVM.ImageIds is null)
            {
                productVM.ImageIds = new();
            }
            List<ProductImage> deletedImages = existed.ProductImages
                .Where(pi => !productVM.ImageIds
                .Exists(imgId => pi.Id == imgId) && pi.IsPrimary==null)
                .ToList();
            deletedImages
                .ForEach(di => di.Image
            .DeleteFile(_env.WebRootPath, "assets", "images", "website-images"));

            _context.ProductImages
                .RemoveRange(deletedImages);

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
            if (productVM.AdditionalPhotos is not null)
            {
                string message = string.Empty;
                foreach (IFormFile file in productVM.AdditionalPhotos)
                {
                    if (!file.ValidateType("image/"))
                    {

                        message += $"<p class=\"text-warning\">{file.FileName} file type is incorrect</p>";
                        continue;
                    }
                    if (!file.ValidateSize(FileSize.MB, 1))
                    {
                        message += $"<p class=\"text-warning\">{file.FileName} file size is incorrect</p>"; ;
                        continue;
                    }

                    existed.ProductImages.Add(new ProductImage()
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                        IsPrimary = null,
                        CreatedAt = DateTime.Now
                    }
                    );
                }
                TempData["ImageWarning"] = message;
            }

            existed.Name = productVM.Name;
            existed.Sku = productVM.Sku;
            existed.Description = productVM.Description;
            existed.Price= productVM.Price.Value;
            existed.CategoryId=productVM.CategoryId.Value;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id < 1) return BadRequest();
            Product product = await _context.Products.Include(p=>p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
            if(product is null) return NotFound();
            product.ProductImages.ForEach(pi => pi.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images"));
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null || id < 1)
            {
                return BadRequest();
            }
            Product? existed = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductTags)
                .Include(p => p.ProductColors)
                .Include(p => p.ProductSizes)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null)
            {
                return NotFound();
            }
            DetailsProductVM productVM = new()
            {
                Name = existed.Name,
                Sku = existed.Sku,
                Description = existed.Description,
                CategoryId = existed.CategoryId,
                Price = existed.Price,
                TagIds = existed.ProductTags.Select(pt => pt.TagId).ToList(),
                ColorIds = existed.ProductColors.Select(pc => pc.ColorId).ToList(),
                SizeIds = existed.ProductSizes.Select(ps => ps.SizeId).ToList(),
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                Colors = await _context.Colors.ToListAsync(),
                Sizes = await _context.Sizes.ToListAsync(),
                ProductImages = existed.ProductImages
            };
            return View(productVM);
        }
    }
}
