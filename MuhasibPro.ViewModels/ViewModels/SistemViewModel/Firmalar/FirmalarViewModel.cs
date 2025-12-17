using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.AppServices;
using Muhasib.Business.Services.Contracts.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.MaliDonemler;

namespace MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar;
public class FirmalarViewModel : ViewModelBase
{
    public FirmalarViewModel(ICommonServices commonServices, IFirmaService firmaService, IFilePickerService filePickerService, IMaliDonemService maliDonemService) : base(commonServices)
    {
        FirmaService = firmaService;
        MaliDonemService = maliDonemService;

        FirmaList = new FirmaListViewModel(commonServices, FirmaService);
        FirmaDetails = new FirmaDetailsViewModel(commonServices, filePickerService, FirmaService);
        MaliDonemList = new MaliDonemListViewModel(commonServices,MaliDonemService);
        
    }
    public IFirmaService FirmaService { get; }
    public IMaliDonemService MaliDonemService { get; }
    public FirmaListViewModel FirmaList { get; set; }
    public FirmaDetailsViewModel FirmaDetails { get; set; }
    public MaliDonemListViewModel MaliDonemList { get; set; }
    

    private string Header = "Firma";
    public async Task LoadAsync(FirmaListArgs args)
    {
        await FirmaList.LoadAsync(args);
    }
    public void Unload()
    {
        FirmaDetails.CancelEdit();
        FirmaList.Unload();
    }
    public void Subscribe()
    {
        MessageService.Subscribe<FirmaListViewModel>(this, OnMessage);
        FirmaList.Subscribe();
        FirmaDetails.Subscribe();
        MaliDonemList.Subscribe();
    }
    public void Unsubscribe()
    {
        MessageService.Unsubscribe(this);
        FirmaList.Unsubscribe();
        FirmaDetails.Unsubscribe();
        MaliDonemList.Unsubscribe();

    }
    private async void OnMessage(FirmaListViewModel viewModel, string message, object args)
    {
        if (viewModel == FirmaList && message == "ItemSelected")
        {
            await ContextService.RunAsync(() =>
            {
                OnItemSelected();
            });
        }
    }

    private async void OnItemSelected()
    {
        if (FirmaDetails.IsEditMode)
        {
            StatusReady();
            FirmaDetails.CancelEdit();
        }
        //CustomerOrders.IsMultipleSelection = false;
        var selected = FirmaList.SelectedItem;
        if (!FirmaList.IsMultipleSelection)
        {
            if (selected != null && !selected.IsEmpty)
            {
                await PopulateDetails(selected);
                await PopulateMaliDonem(selected);
            }
        }
        FirmaDetails.Item = selected;
    }

    private async Task PopulateDetails(FirmaModel selected)
    {
        try
        {
            var model = await FirmaService.GetByFirmaIdAsync(selected.Id);
            selected.Merge(model.Data);
        }
        catch (Exception ex)
        {
            LogSistemException($"{Header}lar", $"{Header} Detay", ex);
        }
    }
    private async Task PopulateMaliDonem(FirmaModel selectedItem)
    {
        try
        {
            if(selectedItem != null)
            {
                await MaliDonemList.LoadAsync(new MaliDonemListArgs { FirmaId = selectedItem.Id },silent:true);
            }
        }
        catch (Exception ex)
        {
            LogSistemException("Firmalar", "Mali Dönemler", ex);
        }
    }
}
