using Muhasebe.Business.Models.DegerlerModel;
using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Helpers;
using MuhasibPro.Contracts.CommonServices;
using MuhasibPro.Infrastructure.Common;
using MuhasibPro.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.Common;
using System.Linq.Expressions;

namespace MuhasibPro.ViewModels.Logging.SistemLogs
{
    #region SistemLogListArgs
    public class SistemLogListArgs
    {
        static public SistemLogListArgs CreateEmpty() => new SistemLogListArgs { IsEmpty = true };

        public SistemLogListArgs()
        {
            OrderByDesc = r => r.KayitTarihi;
        }

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
        public SistemLogListArgs ViewModelArgs { get; private set; }

        public async Task LoadAsync(SistemLogListArgs args)
        {
            ViewModelArgs = args ?? SistemLogListArgs.CreateEmpty();
            Query = ViewModelArgs.Query;

            StartStatusMessage("Loading logs...");
            if (await RefreshAsync())
            {
                EndStatusMessage("Logs loaded");
            }
        }
        public void Unload()
        {
            ViewModelArgs.Query = Query;
        }

        public void Subscribe()
        {
            MessageService.Subscribe<SistemLogListViewModel>(this, OnMessage);
            MessageService.Subscribe<SistemLogDetailsViewModel>(this, OnMessage);
            MessageService.Subscribe<ILogService, SistemLog>(this, OnLogServiceMessage);
        }
        public void Unsubscribe()
        {
            MessageService.Unsubscribe(this);
        }

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
                Items = new List<SistemLogModel>();
                StatusError($"Günlükler yüklenirken hata oluştu: {ex.Message}");
                LogSistemException("SistemLogs", "Yenile", ex);
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

        private async Task<IList<SistemLogModel>> GetItemsAsync()
        {
            if (!ViewModelArgs.IsEmpty)
            {
                DataRequest<SistemLog> request = BuildDataRequest();
                return await LogService.GetSistemLogsAsync(request);
            }
            return new List<SistemLogModel>();
        }

        protected override void OnNew()
        {
            throw new NotImplementedException();
        }

        protected override async void OnRefresh()
        {
            StartStatusMessage("Loading logs...");
            if (await RefreshAsync())
            {
                EndStatusMessage("Logs loaded");
            }
        }

        protected override async void OnDeleteSelection()
        {
            StatusReady();
            if (await DialogService.ShowConfirmationAsync("Silmeyi Onayla", "Seçili günlükleri silmek istediğinizden emin misiniz?", "Tamam", "İptal"))
            {
                int count = 0;
                try
                {
                    if (SelectedIndexRanges != null)
                    {
                        count = SelectedIndexRanges.Sum(r => r.Length);
                        StartStatusMessage($"{count} Günlük siliniyor...");
                        await DeleteRangesAsync(SelectedIndexRanges);
                        MessageService.Send(this, "ItemRangesDeleted", SelectedIndexRanges);
                    }
                    else if (SelectedItems != null)
                    {
                        count = SelectedItems.Count();
                        StartStatusMessage($"{count} Günlük siliniyor...");
                        await DeleteItemsAsync(SelectedItems);
                        MessageService.Send(this, "ItemsDeleted", SelectedItems);
                    }
                }
                catch (Exception ex)
                {
                    StatusError($"{count} Günlük silinirken hata oluştu: {ex.Message}");
                    LogSistemException("SistemLogs", "Sil", ex);
                    count = 0;
                }
                await RefreshAsync();
                SelectedIndexRanges = null;
                SelectedItems = null;
                if (count > 0)
                {
                    EndStatusMessage($"{count} günlük silindi");
                }
            }
        }

        private async Task DeleteItemsAsync(IEnumerable<SistemLogModel> models)
        {
            foreach (var model in models)
            {
                await LogService.SistemLogService.DeleteLogAsync(model);
            }
        }

        private async Task DeleteRangesAsync(IEnumerable<IndexRange> ranges)
        {
            DataRequest<SistemLog> request = BuildDataRequest();
            foreach (var range in ranges)
            {
                await LogService.SistemLogService.DeleteLogRangeAsync(range.Index, range.Length, request);
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

        private async void OnMessage(ViewModelBase sender, string message, object args)
        {
            switch (message)
            {
                case "NewItemSaved":
                case "ItemDeleted":
                case "ItemsDeleted":
                case "ItemRangesDeleted":
                    await ContextService.RunAsync(async () =>
                    {
                        await RefreshAsync();
                    });
                    break;
            }
        }

        private async void OnLogServiceMessage(ILogService logService, string message, SistemLog log)
        {
            if (message == "LogAdded")
            {
                await ContextService.RunAsync(async () =>
                {
                    await RefreshAsync();
                });
            }
        }
    }
}
