using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.SistemServices;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.MaliDonemler;

namespace MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar
{
    public class FirmaDetailsWithMaliDonemlerViewModel : ViewModelBase
    {
        public FirmaDetailsWithMaliDonemlerViewModel(ICommonServices commonServices,IFirmaService firmaService,IFilePickerService filePickerService, IMaliDonemService maliDonemService) : base(commonServices)
        {
            FirmaDetails = new FirmaDetailsViewModel(commonServices,filePickerService,firmaService);
            MaliDonemList = new MaliDonemListViewModel(commonServices, maliDonemService);
        }
        public FirmaDetailsViewModel FirmaDetails { get; set; }
        public MaliDonemListViewModel MaliDonemList { get; set; }
        public async Task LoadAsync(FirmaDetailsArgs args) 
        {
            await FirmaDetails.LoadAsync(args);

            long firmaId = args?.FirmaId ?? 0;
            if (firmaId > 0) 
            {
                await MaliDonemList.LoadAsync(new MaliDonemListArgs { FirmaId = args.FirmaId },silent:true);
            }
            else
            {
                await MaliDonemList.LoadAsync(new MaliDonemListArgs { IsEmpty = true },silent:true);
            }
        }
        public void Unload()
        {
            FirmaDetails.CancelEdit();
            FirmaDetails.Unload();
            MaliDonemList.Unload();
        }

        public void Subscribe()
        {
            MessageService.Subscribe<FirmaDetailsViewModel, FirmaModel>(this, OnMessage);
            FirmaDetails.Subscribe();
            MaliDonemList.Subscribe();
        }

        public void Unsubscribe()
        {
            MessageService.Unsubscribe(this);
            FirmaDetails.Unsubscribe();
            MaliDonemList.Unsubscribe();
        }
        private async void OnMessage(FirmaDetailsViewModel viewModel, string message, FirmaModel firma)
        {
            if (viewModel == FirmaDetails && message == "ItemChanged")
            {
                await MaliDonemList.LoadAsync(new MaliDonemListArgs { FirmaId = firma.Id });
            }
        }

    }
}
