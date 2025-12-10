using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pronia_MVC.DAL;
using Pronia_MVC.Models;
using Pronia_MVC.Services.Implementations;
using Pronia_MVC.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});
builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequiredLength = 8;
    opt.Password.RequireNonAlphanumeric = false;
    opt.User.RequireUniqueEmail = true;
    opt.Lockout.MaxFailedAccessAttempts = 3;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.AddScoped<ILayoutService,LayoutServices>();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllerRoute(
    "default",
    "{area:exists}/{controller=home}/{action=index}/{id?}");
app.MapControllerRoute(
    "default",
    "{controller=home}/{action=index}/{id?}");


app.Run();
