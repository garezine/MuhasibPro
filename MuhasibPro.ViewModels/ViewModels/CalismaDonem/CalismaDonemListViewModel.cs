using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls.Primitives;
using Muhasebe.Business.Models.SistemModel;
using MuhasibPro.ViewModels.Contracts.CommonServices;
using MuhasibPro.ViewModels.ViewModels.Common;
using System.Linq.Expressions;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.CalismaDonem
{
    public class CalismaDonemListArgs
    {
        public static CalismaDonemListArgs CreateEmpty() => new CalismaDonemListArgs { IsEmpty = true };

        public CalismaDonemListArgs()
        {
            OrderByDesc = r => r.MaliDonem;
        }
        public bool IsEmpty { get; set; }
        public long FirmaID { get; set; }
        public string Query { get; set; }
        public Expression<Func<MaliDonemModel, object>> OrderBy { get; set; }
        public Expression<Func<MaliDonemModel, object>> OrderByDesc { get; set; }
    }
    public class CalismaDonemListViewModel : GenericListViewModel<MaliDonemModel>
    {
        public CalismaDonemListViewModel(ICommonServices commonServices) : base(commonServices)
        {
        }
        public ICommand OpenInNewViewCommand => new RelayCommand(OnOpenInNewView);
        private async void OnOpenInNewView()
        {
            if (SelectedItem != null)
            {
                await NavigationService.CreateNewViewAsync<CalismaDonemDetailsViewModel>(new CalismaDonemDetailsArgs { CalismaDonemID = SelectedItem.Id });
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
    }
}
