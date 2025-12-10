using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Pronia_MVC.Services.Interfaces
{
    public interface ILayoutService
    {
        Task<Dictionary<string, string>> GetSettingsAsync();
    }
}
