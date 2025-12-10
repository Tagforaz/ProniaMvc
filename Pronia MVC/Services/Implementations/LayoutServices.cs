using Microsoft.EntityFrameworkCore;
using Pronia_MVC.DAL;
using Pronia_MVC.Services.Interfaces;

namespace Pronia_MVC.Services.Implementations
{
    public class LayoutServices : ILayoutService
    {
        private readonly AppDbContext _context;

        public LayoutServices(AppDbContext context)
        {
            _context = context;
        }


        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);

            return settings;
        }


    }
}
