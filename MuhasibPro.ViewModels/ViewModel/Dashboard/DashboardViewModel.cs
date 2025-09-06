using MuhasibPro.Infrastructure.Infrastructure.ViewModels;
using MuhasibPro.Infrastructure.Services.Abstract.Common;

namespace MuhasibPro.ViewModels.ViewModel.Dashboard
{
    public class DashboardViewModel : ViewModelBase
    {

        public DashboardViewModel(ICommonServices commonServices) : base(commonServices)
        {
        }
    }
}
