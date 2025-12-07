using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Pronia_MVC.Models;
using Pronia_MVC.Utilities.Extensions;
using Pronia_MVC.ViewModels;

namespace Pronia_MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IWebHostEnvironment _env;

        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager,IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM userVM)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }
            if (userVM.Photo is not null)
            {
                if (!userVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(CreateSlideVM.Photo), "File type is incorrect");
                    return View();
                }
                if (!userVM.Photo.ValidateSize(Utilities.Enums.FileSize.KB, 2))
                {
                    ModelState.AddModelError(nameof(CreateSlideVM.Photo), "File size is incorrect");
                    return View();
                }
            }
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            DateOnly birthday = userVM.Birthday; 

            int age = today.Year - birthday.Year;

            if (today < new DateOnly(today.Year, birthday.Month, birthday.Day))
            {
                age--;
            }

            if (age < 18)
            {
                ModelState.AddModelError(nameof(RegisterVM.Birthday), "Age under 18 can not register.");
                return View(userVM);
            }
          
            AppUser user = new AppUser
            {

                UserName = userVM.UserName,
                Email = userVM.Email,
                Name = userVM.Name,
                Surname = userVM.Surname,
                Birthday = userVM.Birthday,
                Gender = userVM.Gender,
                PhoneNumber = userVM.PhoneNumber
                
                
            };
            if(userVM.Photo is not null)
            {
                user.Image = await userVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
            }
            else
            {
                if (userVM.Gender == Gender.Female)
                {
                    user.Image = "pfavatar.png";
                }
                else
                {
                    user.Image = "ppavatar.png";
                }
            }
        
            IdentityResult result= await _userManager.CreateAsync(user,userVM.Password);
            if(!result.Succeeded)
            {
                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }
            await _signInManager.SignInAsync(user, false); 
            return RedirectToAction("Index", "Home");
        }
    }
}
