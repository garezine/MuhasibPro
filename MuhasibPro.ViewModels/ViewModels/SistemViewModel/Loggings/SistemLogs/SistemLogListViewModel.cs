using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.Common;
using Muhasib.Domain.Entities.SistemEntity;
using Muhasib.Domain.Enum;
using Muhasib.Domain.Exceptions;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Linq.Expressions;


namespace MuhasibPro.ViewModels.ViewModels.SistemViewModel.Loggings.SistemLogs
{
    #region SistemLogListArgs
    public class SistemLogListArgs
    {
        static public SistemLogListArgs CreateEmpty() => new SistemLogListArgs { IsEmpty = true };

        public SistemLogListArgs() { OrderByDesc = r => r.KayitTarihi; }

        public bool IsEmpty { get; set; }

        public string Query { get; set; }

        public Expression<Func<SistemLog, object>> OrderBy { get; set; }

        public Expression<Func<SistemLog, object>> OrderByDesc { get; set; }
    }
    #endregion

    public class SistemLogListViewModel : GenericListViewModel<SistemLogModel>
    {
        public SistemLogListViewModel(ICommonServices commonServices) : base(commonServices)
        {
        }

        private string Header => "Günlük";

        public SistemLogListArgs ViewModelArgs { get; private set; }

        public async Task LoadAsync(SistemLogListArgs args)
        {
            ViewModelArgs = args ?? SistemLogListArgs.CreateEmpty();
            Query = ViewModelArgs.Query;
            await ExecuteActionAsync(
                action: async () => await RefreshAsync(),
                startMessage: $"{Header} listesi yükleniyor....",
                startMessageType: StatusMessageType.Info,
                successMessage: $"{Header} listesi yüklendi");
        }

        public void Unload() { ViewModelArgs.Query = Query; }

        public void Subscribe()
        {
            MessageService.Subscribe<SistemLogListViewModel>(this, OnMessage);
            MessageService.Subscribe<SistemLogDetailsViewModel>(this, OnMessage);
            MessageService.Subscribe<ILogService, SistemLog>(this, OnLogServiceMessage);
        }

        public void Unsubscribe() { MessageService.Unsubscribe(this); }
        public SistemLogListArgs CreateArgs()
        {
            return new SistemLogListArgs
            {
                Query = Query,
                OrderBy = ViewModelArgs.OrderBy,
                OrderByDesc = ViewModelArgs.OrderByDesc
            };
        }

        public async Task<bool> RefreshAsync()
        {
            try
            {
                await LoadDataAsync();
                NotifyPropertyChanged(nameof(Title));  // ✅ SADECE BAŞARILI DURUMDA
                return true;
            }
           
            catch (Exception ex)
            {
                StatusError($"{Header} listesi yenilenirken beklenmeyen hata");
                LogSistemException($"{Header}", "Yenile", ex);
                return false;
            }
        }

        protected override async Task LoadDataAsync()
        {
            if(!ViewModelArgs.IsEmpty)
            {
                DataRequest<SistemLog> request = BuildDataRequest();
                ItemsCount = await LogService.SistemLogService.GetSistemLogsCountAsync(request);
                Items = await LogService.SistemLogService
                    .GetSistemLogsAsync((CurrentPage - 1) * PageSize, PageSize, request);
                await ContextService.RunAsync(
                    () =>
                    {
                        // ESKİ: ItemsSource = new ObservableCollection() - SCROLL KAYBOLUYOR
                        // YENİ: Mevcut collection'ı güncelle - SCROLL KORUNUYOR
                        ItemsSource.Clear();
                        foreach(var item in Items)
                        {
                            ItemsSource.Add(item);
                        }
                        if(!IsMultipleSelection && ItemsSource.Count > 0)
                        {
                            SelectedItem = ItemsSource.FirstOrDefault();
                        }
                    });
            } else
            {
                Items = new List<SistemLogModel>();
                await ContextService.RunAsync(
                    () =>
                    {
                        ItemsSource?.Clear();
                    });
                ItemsCount = 0;
                SelectedItem = null;
            }
        }

        private DataRequest<SistemLog> BuildDataRequest()
        {
            return new DataRequest<SistemLog>()
            {
                Query = Query,
                OrderBy = ViewModelArgs.OrderBy,
                OrderByDesc = ViewModelArgs.OrderByDesc
            };
        }
        protected override void OnNew() { throw new NotImplementedException(); }

        protected override async void OnRefresh()
        {
            await ExecuteActionAsync(
            action: async () => await RefreshAsync(),
            startMessage: $"{Header} listesi yenileniyor...",
            startMessageType: StatusMessageType.Refreshing,
            successMessage: $"{Header} listesi yenilendi");
        }

        protected override async void OnDeleteSelection()
        {
            StatusReady();
            if (await DialogService.ShowAsync(
                "Silmeyi Onayla",
                $"Seçili {Header}leri silmek istediğinizden emin misiniz?",
                "Tamam",
                "İptal"))
            {
                int count = 0;
                try
                {
                    if(SelectedIndexRanges != null)
                    {
                        count = SelectedIndexRanges.Sum(r => r.Length);
                        StartProgressWithPercent($"{count} {Header} siliniyor...");

                        // 1. ÖNCE veritabanından sil
                        await DeleteRangesAsync(SelectedIndexRanges);
                        MessageService.Send(this, "ItemRangesDeleted", SelectedIndexRanges);

                        // 2. SONRA UI'dan animasyonlu şekilde sil
                        await RemoveFromUIWithAnimationAsync(SelectedIndexRanges);
                    } else if(SelectedItems != null)
                    {
                        count = SelectedItems.Count();
                        StartProgressWithPercent($"{count} {Header} siliniyor...");

                        // 1. ÖNCE veritabanından sil
                        await DeleteItemsAsync(SelectedItems);
                        MessageService.Send(this, "ItemsDeleted", SelectedItems);

                        // 2. SONRA UI'dan animasyonlu şekilde sil
                        await RemoveItemsFromUIWithAnimationAsync(SelectedItems);
                    }
                }
                
                catch (Exception ex)
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
                if (count > 0)
                {
                    StatusActionMessage($"{count} {Header} silindi", StatusMessageType.Deleting, autoHide:-1);
                }
            }
        }       

        private async Task DeleteItemsAsync(IEnumerable<SistemLogModel> models)
        {
            var itemList = models.ToList();
            foreach(var model in itemList)
            {
                await LogService.SistemLogService.DeleteSistemLogAsync(model);
            }
        }

        private async Task DeleteRangesAsync(IEnumerable<IndexRange> ranges)
        {
            DataRequest<SistemLog> request = BuildDataRequest();
            foreach (var range in ranges)
            {
                await LogService.SistemLogService.DeleteSistemLogRangeAsync(range.Index, range.Length, request);
            }
            
        }

        private async void OnMessage(ViewModelBase sender, string message, object args)
        {
            switch(message)
            {
                case "NewItemSaved":
                case "ItemDeleted":
                    if(SelectedItem != null)
                    {
                        ItemsSource.Remove(SelectedItem);
                    }
                    break;
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

        private async void OnLogServiceMessage(ILogService logService, string message, SistemLog log)
        {
            if(message == "LogAdded")
            {
                await ContextService.RunAsync(
                    async () =>
                    {
                        await RefreshAsync();
                    });
            }
        }
    }
}
