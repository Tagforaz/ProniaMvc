using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia_MVC.Models;
using Pronia_MVC.Utilities.Enums;
using Pronia_MVC.Utilities.Extensions;
using Pronia_MVC.ViewModels;

namespace Pronia_MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IWebHostEnvironment env,
            RoleManager<IdentityRole> roleManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
            _roleManager = roleManager;
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM userVM, string? returnUrl)
        {
            if (!ModelState.IsValid)
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
                if (!userVM.Photo.ValidateSize(Utilities.Enums.FileSize.MB, 2))
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
            if (userVM.Photo is not null)
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

            IdentityResult result = await _userManager.CreateAsync(user, userVM.Password);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }
            await _userManager.AddToRoleAsync(user, UserRole.Member.ToString());
            await _signInManager.SignInAsync(user, false);
            if (returnUrl is not null)
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM userVM, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            AppUser user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == userVM.UserNameOrEmail || u.Email == userVM.UserNameOrEmail);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Username,Email or Password is incorrect");
                return View();
            }
            var result = await _signInManager.PasswordSignInAsync(user, userVM.Password, userVM.IsPersistant, true);
            if (!result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account is blocked please try later");
                return View();
            }
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Username,Email or Password is incorrect");
                return View();
            }
            user.LockoutEnd = null;
            if (returnUrl is not null)
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");

        }
        //[Authorize]
        public async Task<IActionResult> LogOut(string? returnUrl)
        {
            await _signInManager.SignOutAsync();
            if (returnUrl is not null)
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            AppUser? user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound();
            }
            ProfileDetailsVM profileDetailsVM = new()
            {
                UserName = user.UserName,
                Name = user.Name,
                Surname = user.Surname,
                Birthday = user.Birthday,
                Image = user.Image,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };
            return View(profileDetailsVM);
        }
        public async Task<IActionResult> CreateRoles()
        {
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                if (!await _roleManager.RoleExistsAsync(role.ToString()))
                {
                    IdentityRole identityRole = new()
                    {
                        Name = role.ToString(),
                    };
                    await _roleManager.CreateAsync(identityRole);
                }

            }
            return RedirectToAction("Index", "Home");

        }
    }
}
