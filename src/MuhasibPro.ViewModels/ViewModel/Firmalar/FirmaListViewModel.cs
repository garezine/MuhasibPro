using CommunityToolkit.Mvvm.Input;
using Muhasebe.Business.Models.DbModel.AppModel;
using Muhasebe.Business.Services.Abstract.App;
using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.SistemDb;
using MuhasibPro.Core.Infrastructure.Common;
using MuhasibPro.Core.Infrastructure.ViewModels;
using MuhasibPro.Core.Services.Abstract.Common;
using MuhasibPro.Infrastructure.ViewModels.Common;
using System.Linq.Expressions;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModel.Firmalar;

public class FirmaListArgs
{
    public static FirmaListArgs CreateEmpty() => new() { IsEmpty = true };

    public FirmaListArgs()
    {
        OrderBy = r => r.KisaUnvani;        
    }
    public bool IsEmpty { get; set; }

    public string Query { get; set; }

    public Expression<Func<Firma, object>> OrderBy { get; set; }
    public Expression<Func<Firma, object>> OrderByDesc { get; set; }
    public Expression<Func<Firma, object>>[] Includes { get; set; }

}

public class FirmaListViewModel : GenericListViewModel<FirmaModel>
{
    public FirmaListViewModel(ICommonServices commonServices, IFirmaService firmaService) : base(commonServices)
    { FirmaService = firmaService; }

    public IFirmaService FirmaService { get; }

    public FirmaListArgs ViewModelArgs { get; private set; }

    public async Task LoadAsync(FirmaListArgs args)
    {
        ViewModelArgs = args ?? FirmaListArgs.CreateEmpty();
        Query = ViewModelArgs.Query;       
        StartStatusMessage("Firmalar yükleniyor....");
        StatusBar.ShowProgress();       
        if (await RefreshAsync())
        {
            StatusBar.HideProgress();
            EndStatusMessage("Firmalar yüklendi");
        }
    }

    public void Unload() { ViewModelArgs.Query = Query; }

    public void Subscribe()
    {
        MessageService.Subscribe<FirmaListViewModel>(this, OnMessage);
        MessageService.Subscribe<FirmaDetailsViewModel>(this, OnMessage);
    }

    public void Unsubscribe() { MessageService.Unsubscribe(this); }
    public FirmaListArgs CreateArgs() 
    {
        return new FirmaListArgs
        {
            Query = Query,
            OrderBy = ViewModelArgs.OrderBy,
            OrderByDesc = ViewModelArgs.OrderByDesc,
            Includes = ViewModelArgs.Includes,

        };
    }

    public async Task<bool> RefreshAsync()
    {
        bool isOk = true;

        Items = null;
        ItemsCount = 0;
        SelectedItem = null;        

        try
        {           
            Items = await GetItemsAsync();
        }
        catch (Exception ex)
        {           
            Items = new List<FirmaModel>();
            StatusError($"Firmalar yüklenirken hata oluştu: {ex.Message}");
            LogException("Firmalar", "Yenile", ex);
            isOk = false;
        }

        ItemsCount = Items.Count;
        if (!IsMultipleSelection)
        {
            SelectedItem = Items.FirstOrDefault();
        }
        NotifyPropertyChanged(nameof(Title));        
        return isOk;
    }

    private async Task<IList<FirmaModel>> GetItemsAsync()
    {
        if (!ViewModelArgs.IsEmpty)
        {
            DataRequest<Firma> request = BuildDataRequest();
            return await FirmaService.GetFirmalarAsync(request);
        }
        return new List<FirmaModel>();
    }
    private DataRequest<Firma> BuildDataRequest()
    {
        
        return new DataRequest<Firma>()
        {
            Query = Query,
            OrderBy = ViewModelArgs.OrderBy,
            OrderByDesc = ViewModelArgs.OrderByDesc,
            Includes=ViewModelArgs.Includes
        };
    }

    public ICommand OpenInNewViewCommand => new RelayCommand(OnOpenInNewView);

    private async void OnOpenInNewView()
    {
        if (SelectedItem != null)
        {
            await NavigationService.CreateNewViewAsync<FirmaDetailsViewModel>(
                new FirmaDetailsArgs { FirmaId = SelectedItem.Id },customTitle:"Firmalar");
        }
    }

    protected override async void OnNew()
    {
        if (IsMainWindow)
        {
            await NavigationService.CreateNewViewAsync<FirmaDetailsViewModel>(new FirmaDetailsArgs(),customTitle:"Yeni Firma");
        }
        else
        {
            
            NavigationService.Navigate<FirmaDetailsViewModel>(new FirmaDetailsArgs());
        }

        StatusReady();
    }


    protected async override void OnRefresh()
    {
        StartStatusMessage("Firmalar yükleniyor...");
        StatusBar.IsStatusProgress = true;
        if (await RefreshAsync())
        {
            StatusBar.IsStatusProgress = false;
            EndStatusMessage("Firmalar yüklendi");
        }
    }

    protected async override void OnDeleteSelection()
    {
        StatusReady();
        if (await DialogService.ShowConfirmationAsync(
            "Silmeyi Onayla",
            "Seçilen firmayı silmek istediğinize emin misiniz?",
            "Evet",
            "İptal"))
        {
            int count = 0;
            try
            {
                if (SelectedIndexRanges != null)
                {
                    count = SelectedIndexRanges.Sum(r => r.Length);
                    StartStatusMessage($"{count} firma siliniyor...");
                    await DeleteRangesAsync(SelectedIndexRanges);
                    MessageService.Send(this, "ItemRangesDeleted", SelectedIndexRanges);
                }
                else if (SelectedItems != null)
                {
                    count = SelectedItems.Count();
                    StartStatusMessage($"{count} firma siliniyor...");
                    await DeleteItemsAsync(SelectedItems);
                    MessageService.Send(this, "ItemsDeleted", SelectedItems);
                }
            }
            catch (Exception ex)
            {
                StatusError($"{count} firma silme hatası: {ex.Message}");
                LogException("Firmalar", "Sil", ex);
                count = 0;
            }
            await RefreshAsync();
            SelectedIndexRanges = null;
            SelectedItems = null;
            if (count > 0)
            {
                EndStatusMessage($"{count} firma silindi");
            }
        }
    }

    private async Task DeleteItemsAsync(IEnumerable<FirmaModel> models)
    {
        foreach (var model in models)
        {
            await FirmaService.DeleteFirmaAsync(model);
        }
    }

    private async Task DeleteRangesAsync(IEnumerable<IndexRange> ranges)
    {
        DataRequest<Firma> request = BuildDataRequest();
        foreach (var range in ranges)
        {
            await FirmaService.DeleteFirmaRangeAsync(range.Index, range.Length, request);
        }
    }

    private async void OnMessage(ViewModelBase sender, string message, object args)
    {
        switch (message)
        {
            case "NewItemSaved":
            case "ItemDeleted":
            case "ItemsDeleted":
            case "ItemRangesDeleted":
                await ContextService.RunAsync(
                    async () =>
                    {
                        await RefreshAsync();
                    });
                break;
        }
    }
}
