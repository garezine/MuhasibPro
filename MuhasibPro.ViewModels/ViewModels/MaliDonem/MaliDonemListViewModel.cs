using Muhasib.Business.Models.SistemModel;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Linq.Expressions;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.MaliDonem
{
    public class CalismaDonemListArgs
    {
        public static CalismaDonemListArgs CreateEmpty() => new CalismaDonemListArgs { IsEmpty = true };

        public CalismaDonemListArgs()
        {
            OrderByDesc = r => r.MaliYil;
        }
        public bool IsEmpty { get; set; }
        public long FirmaID { get; set; }
        public string Query { get; set; }
        public Expression<Func<MaliDonemModel, object>> OrderBy { get; set; }
        public Expression<Func<MaliDonemModel, object>> OrderByDesc { get; set; }
    }
    public class MaliDonemListViewModel : GenericListViewModel<MaliDonemModel>
    {
        public MaliDonemListViewModel(ICommonServices commonServices) : base(commonServices)
        {
        }

        public ICommand OpenInNewViewCommand => new RelayCommand(OnOpenInNewView);
        private async void OnOpenInNewView()
        {
            if (SelectedItem != null)
            {
                await NavigationService.CreateNewViewAsync<MaliDonemDetailsViewModel>(new CalismaDonemDetailsArgs { CalismaDonemID = SelectedItem.Id });
            }
        }
        protected override void OnDeleteSelection()
        {
            throw new NotImplementedException();
        }

        protected override void OnNew()
        {
            throw new NotImplementedException();
        }

        protected override void OnRefresh()
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> LoadDataAsync()
        {
            throw new NotImplementedException();
        }
    }
}
