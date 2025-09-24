using Microsoft.UI.Xaml;
using MuhasibPro.ViewModels.Contracts.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Settings
{
    public class SettingsViewModel : ViewModelBase
    {

        public SettingsViewModel(ICommonServices commonServices) : base(commonServices)
        {
        }

        private ElementTheme _elementTheme;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }
            set { Set(ref _elementTheme, value); }
        }



    }
}
