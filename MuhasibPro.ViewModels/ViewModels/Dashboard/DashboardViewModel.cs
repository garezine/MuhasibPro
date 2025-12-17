using Muhasib.Business.Services.Contracts.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Dashboard
{
    public class DashboardViewModel : ViewModelBase
    {

        public DashboardViewModel(ICommonServices commonServices) : base(commonServices)
        {
        }
    }
}
