using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.SistemServices;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.MaliDonem;

namespace MuhasibPro.ViewModels.ViewModels.Firmalar;
public class FirmalarViewModel : ViewModelBase
{
    public FirmalarViewModel(ICommonServices commonServices, IFirmaService firmaService, IFilePickerService filePickerService) : base(commonServices)
    {
        FirmaService = firmaService;

        FirmaList = new FirmaListViewModel(commonServices, FirmaService);
        FirmaDetails = new FirmaDetailsViewModel(commonServices, filePickerService, FirmaService);
        FirmaCalismaDonemler = new MaliDonemListViewModel(commonServices);
    }
    public IFirmaService FirmaService { get; }
    public FirmaListViewModel FirmaList { get; set; }
    public FirmaDetailsViewModel FirmaDetails { get; set; }
    public MaliDonemListViewModel FirmaCalismaDonemler { get; set; }

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
    }
    public void Unsubscribe()
    {
        MessageService.Unsubscribe(this);
        FirmaList.Unsubscribe();
        FirmaDetails.Unsubscribe();

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
                //await PopulateOrders(selected);
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
}
