using Microsoft.UI.Xaml;
using MuhasibPro.Infrastructure.Infrastructure.ViewModels;
using MuhasibPro.Infrastructure.Services.Abstract.Common;

namespace MuhasibPro.ViewModels.ViewModel.Settings
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
