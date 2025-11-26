using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.SistemServices;
using Muhasib.Data.Common;
using Muhasib.Domain.Entities.SistemEntity;
using Muhasib.Domain.Enum;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Linq.Expressions;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.SistemViewModel.MaliDonemler
{
    public class MaliDonemListArgs
    {
        public static MaliDonemListArgs CreateEmpty() => new() { IsEmpty = true };

        public MaliDonemListArgs() { OrderBy = r => r.MaliYil; }

        public bool IsEmpty { get; set; }

        public string Query { get; set; }

        public long FirmaId { get; set; }

        public Expression<Func<MaliDonem, object>> OrderBy { get; set; }

        public Expression<Func<MaliDonem, object>> OrderByDesc { get; set; }

        public Expression<Func<MaliDonem, object>>[] Includes { get; set; }
    }

    public class MaliDonemListViewModel : GenericListViewModel<MaliDonemModel>
    {
        public MaliDonemListViewModel(ICommonServices commonServices, IMaliDonemService maliDonemService) : base(
            commonServices)
        { MaliDonemService = maliDonemService; }

        public IMaliDonemService MaliDonemService { get; }

        private string Header => "Mali Dönem";

        public MaliDonemListArgs ViewModelArgs { get; private set; }

        public async Task LoadAsync(MaliDonemListArgs args, bool silent = false)
        {
            ViewModelArgs = args ?? MaliDonemListArgs.CreateEmpty();
            Query = ViewModelArgs.Query;
            if(silent)
            {
                await RefreshAsync();
            } else
            {
                await ExecuteActionAsync(
                    action: async () => await RefreshAsync(),
                    startMessage: $"{Header} listesi yükleniyor....",
                    startMessageType: StatusMessageType.Refreshing,
                    successMessage: $"{Header} listesi yüklendi");
            }
        }

        public void Unload() { ViewModelArgs.Query = Query; }

        public void Subscribe()
        {
            MessageService.Subscribe<MaliDonemListViewModel>(this, OnMessage);
            MessageService.Subscribe<MaliDonemDetailsViewModel>(this, OnMessage);
        }

        public void Unsubscribe() { MessageService.Unsubscribe(this); }

        public MaliDonemListArgs CreateArgs()
        {
            return new MaliDonemListArgs
            {
                Query = Query,
                OrderBy = ViewModelArgs.OrderBy,
                OrderByDesc = ViewModelArgs.OrderByDesc,
                Includes = ViewModelArgs.Includes,
                FirmaId = ViewModelArgs.FirmaId,
            };
        }

        public async Task<bool> RefreshAsync()
        {
            try
            {
                await LoadDataAsync();
                NotifyPropertyChanged(nameof(Title));  // ✅ SADECE BAŞARILI DURUMDA
                return true;
            }  catch(Exception ex)
            {
                StatusError($"{Header} listesi yenilenirken beklenmeyen hata");
                LogSistemException($"{Header}", "Yenile", ex);
                return false;
            }
        }

        protected async override Task LoadDataAsync()
        {
            if(!ViewModelArgs.IsEmpty)
            {
                DataRequest<MaliDonem> request = BuildDataRequest();
                // TEK servis call - daha hızlı!
                ItemsCount = await MaliDonemService.GetMaliDonemlerCountAsync(request);
                var items = await MaliDonemService.GetMaliDonemlerAsync(request);
                // Items'e ata ve Count'u Items'den al
                Items = items?.Data;
                if (Items != null)
                {
                    await ContextService.RunAsync(
                        () =>
                        {
                            ItemsSource.Clear();
                            foreach (var item in Items)
                            {
                                ItemsSource.Add(item);
                            }
                            if (!IsMultipleSelection && ItemsSource.Count > 0)
                            {
                                SelectedItem = ItemsSource.FirstOrDefault();
                            }
                        });
                }
            } else
            {
                Items = new List<MaliDonemModel>();      // ← ITEMS'I TEMIZLE
                await ContextService.RunAsync(
                    () =>
                    {
                        ItemsSource?.Clear();
                    });
                ItemsCount = 0;
                SelectedItem = null;
            }
        }

        private DataRequest<MaliDonem> BuildDataRequest()
        {
            var request = new DataRequest<MaliDonem>()
            {
                Query = Query,
                OrderBy = ViewModelArgs.OrderBy,
                OrderByDesc = ViewModelArgs.OrderByDesc,
                Includes = ViewModelArgs.Includes
            };
            if(ViewModelArgs.FirmaId > 0)
            {
                request.Where = (r) => r.FirmaId == ViewModelArgs.FirmaId;
            }
            return request;
        }

        public ICommand OpenInNewViewCommand => new RelayCommand(OnOpenInNewView);

        private async void OnOpenInNewView()
        {
            if(SelectedItem != null)
            {
                await NavigationService.CreateNewViewAsync<MaliDonemDetailsViewModel>(
                    new MaliDonemDetailsArgs { MaliDonemId = SelectedItem.Id },
                    customTitle: $"{Header}'ler");
            }
        }

        protected override async void OnNew()
        {
            if(IsMainWindow)
            {
                await NavigationService.CreateNewViewAsync<MaliDonemDetailsViewModel>(
                    new MaliDonemDetailsArgs { FirmaId = ViewModelArgs.FirmaId },
                    customTitle: $"Yeni {Header}");
            } else
            {
                NavigationService.Navigate<MaliDonemDetailsViewModel>(
                    new MaliDonemDetailsArgs { FirmaId = ViewModelArgs.FirmaId });
            }
            StatusReady();
        }

        protected async override void OnDeleteSelection()
        {
            StatusReady();
            if(await DialogService.ShowAsync(
                "Silmeyi Onayla",
                $"Seçili {Header}(ler) silmek istediğinize emin misiniz?",
                "Evet",
                "İptal"))
            {
                int count = 0;
                try
                {
                    if(SelectedIndexRanges != null)
                    {
                        count = SelectedIndexRanges.Sum(r => r.Length);
                        StartProgressWithPercent($"{count} {Header} siliniyor...");

                        await DeleteRangesAsync(SelectedIndexRanges);
                        MessageService.Send(this, "ItemRangesDeleted", SelectedIndexRanges);
                        await RemoveFromUIWithAnimationAsync(SelectedIndexRanges);
                    } else if(SelectedItems != null)
                    {
                        count = SelectedItems.Count();
                        StartProgressWithPercent($"{count} {Header} siliniyor...");

                        await DeleteItemsAsync(SelectedItems);
                        MessageService.Send(this, "ItemsDeleted", SelectedItems);

                        await RemoveItemsFromUIWithAnimationAsync(SelectedItems);
                    }
                }  catch(Exception ex)
                {
                    StatusError($"{Header} silinirken beklenmeyen hata");
                    LogSistemException($"{Header}", "Sil", ex);
                    StopProgress();
                    count = 0;
                }
                await RefreshAsync();
                SelectedIndexRanges = null;
                SelectedItems = null;
                StopProgress();
                if(count > 0)
                {
                    StatusActionMessage($"{count} {Header} silindi", StatusMessageType.Deleting, autoHide: -1);
                }
            }
        }

        private async Task DeleteItemsAsync(IEnumerable<MaliDonemModel> models)
        {
            foreach(var model in models)
            {
                await MaliDonemService.DeleteMaliDonemAsync(model);
            }
        }

        private async Task DeleteRangesAsync(IEnumerable<IndexRange> ranges)
        {
            DataRequest<MaliDonem> request = BuildDataRequest();
            foreach(var range in ranges)
            {
                await MaliDonemService.DeleteMaliDonemRangeAsync(range.Index, range.Length, request);
            }
        }

        protected async override void OnRefresh()
        {
            await ExecuteActionAsync(
                action: async () => await RefreshAsync(),
                startMessage: $"{Header} listesi yenileniyor...",
                startMessageType: StatusMessageType.Refreshing,
                successMessage: $"{Header} listesi yenilendi");
        }

        private async void OnMessage(ViewModelBase sender, string message, object args)
        {
            switch(message)
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
}
