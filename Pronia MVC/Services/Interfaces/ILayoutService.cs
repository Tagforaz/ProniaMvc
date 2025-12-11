using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Pronia_MVC.ViewModels;

namespace Pronia_MVC.Services.Interfaces
{
    public interface ILayoutService
    {
        Task<Dictionary<string, string>> GetSettingsAsync();
        Task<BasketVM> GetBasketAsync();
    }
}
