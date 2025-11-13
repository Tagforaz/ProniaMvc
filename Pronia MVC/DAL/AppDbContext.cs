using Microsoft.EntityFrameworkCore;
using Pronia_MVC.Models;

namespace Pronia_MVC.DAL
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> option):base(option)
        {}
        public DbSet<Slide> Slides { get; set; }
    }
}
