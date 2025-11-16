using MuhasibPro.ViewModels.Contracts.Services.CommonServices;

namespace MuhasibPro.Services.Infrastructure.CommonServices
{
    public class SettingsService : ISettingsService
    {

        public SettingsService()
        {

        }

        public string GetUserName { get; set; }
    }
}
