using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Settings
{
    public class SettingsViewModel : ViewModelBase
    {

        public SettingsViewModel(ICommonServices commonServices) : base(commonServices)
        {
        }
    }
}
