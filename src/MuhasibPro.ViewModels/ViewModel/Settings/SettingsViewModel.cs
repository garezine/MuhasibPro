using Microsoft.UI.Xaml;
using MuhasibPro.Core.Infrastructure.ViewModels;
using MuhasibPro.Core.Services.Abstract.Common;

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
