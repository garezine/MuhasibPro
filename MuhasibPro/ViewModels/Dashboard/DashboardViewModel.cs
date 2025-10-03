using MuhasibPro.Contracts.CommonServices;
using MuhasibPro.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.Dashboard
{
    public class DashboardViewModel : ViewModelBase
    {

        public DashboardViewModel(ICommonServices commonServices) : base(commonServices)
        {
        }
    }
}
