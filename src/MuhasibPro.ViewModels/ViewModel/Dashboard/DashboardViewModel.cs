using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MuhasibPro.Core.Infrastructure.Update;
using MuhasibPro.Core.Infrastructure.ViewModels;
using MuhasibPro.Core.Services.Common;
using Newtonsoft.Json;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModel.Dashboard
{
    public class DashboardViewModel : ViewModelBase
    {

        public DashboardViewModel(ICommonServices commonServices) : base(commonServices)
        {            
        }   
    }
}
