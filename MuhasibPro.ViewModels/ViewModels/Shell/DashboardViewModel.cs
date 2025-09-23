using MuhasibPro.ViewModels.Contracts.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels.Common;

namespace MuhasibPro.ViewModels.ViewModels.Shell
{
    public class DashboardViewModel : ViewModelBase
    {
        public DashboardViewModel(ICommonServices commonServices) : base(commonServices)
        {
        }
    }
}
