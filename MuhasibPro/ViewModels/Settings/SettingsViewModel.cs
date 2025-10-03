using MuhasibPro.Contracts.CommonServices;
using MuhasibPro.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.Settings
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
