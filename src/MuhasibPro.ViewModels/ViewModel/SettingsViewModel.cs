using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MuhasibPro.Core.Infrastructure.ViewModels;
using MuhasibPro.Core.Services;
using MuhasibPro.Core.Services.Common;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModel
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
