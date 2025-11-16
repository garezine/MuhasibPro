using Muhasib.Business.Models.SistemModel;
using Muhasib.Domain.Enum;
using Muhasib.Domain.Exceptions;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Loggings.SistemLogs
{
    #region SistemLogDetailsArgs
    public class SistemLogDetailsArgs
    {
        static public SistemLogDetailsArgs CreateDefault() => new SistemLogDetailsArgs();

        public long AppLogID { get; set; }
    }
    #endregion

    public class SistemLogDetailsViewModel : GenericDetailsViewModel<SistemLogModel>
    {
        public SistemLogDetailsViewModel(ICommonServices commonServices) : base(commonServices)
        {
        }
        public override string Title => "Sistem Günlüğü";

        private string Header => "Sistem Günlüğü";
        public override bool ItemIsNew => false;

        public SistemLogDetailsArgs ViewModelArgs { get; private set; }

        public async Task LoadAsync(SistemLogDetailsArgs args)
        {
            ViewModelArgs = args ?? SistemLogDetailsArgs.CreateDefault();

            try
            {
                var item = await LogService.SistemLogService.GetSistemLogAsync(ViewModelArgs.AppLogID);
                Item = item ?? new SistemLogModel { Id = 0, IsEmpty = true };
            }
            catch (UserFriendlyException uex)
            {
                StatusError(uex); // ✅ "Hata: 0x08002 - Veritabanı bağlantı hatası"
            }
            catch (Exception ex)
            {
                StatusError(GlobalErrorCode.GeneralError, $"{Header} bilgileri yüklenirken beklenmeyen hata");
                LogSistemException($"{Header}", $"{Header} Detay", ex);
            }
        }
        public void Unload()
        {
            ViewModelArgs.AppLogID = Item?.Id ?? 0;
        }

        public void Subscribe()
        {
            MessageService.Subscribe<SistemLogDetailsViewModel, SistemLogModel>(this, OnDetailsMessage);
            MessageService.Subscribe<SistemLogListViewModel>(this, OnListMessage);
        }
        public void Unsubscribe()
        {
            MessageService.Unsubscribe(this);
        }

        public SistemLogDetailsArgs CreateArgs()
        {
            return new SistemLogDetailsArgs
            {
                AppLogID = Item?.Id ?? 0
            };
        }

        protected override Task<bool> SaveItemAsync(SistemLogModel model)
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> DeleteItemAsync(SistemLogModel model)
        {
            try
            {               
                await LogService.SistemLogService.DeleteSistemLogAsync(model);                
                return true;
            }
            catch (UserFriendlyException uex)
            {
                StatusError(uex);
                return false;
            }
            catch (Exception ex)
            {
                StatusError(GlobalErrorCode.GeneralError, $"{Header} silinirken beklenmeyen hata");
                LogSistemException($"{Header}", "Sil", ex);
                return false;
            }
        }

        protected override async Task<bool> ConfirmDeleteAsync()
        {
            return await DialogService.ShowAsync("Silmeyi Onayla", $"Geçerli {Header}'nü silmek istediğinizden emin misiniz?", "Tamam", "İptal");
        }

        /*
         *  Handle external messages
         ****************************************************************/
        private async void OnDetailsMessage(SistemLogDetailsViewModel sender, string message, SistemLogModel changed)
        {
            var current = Item;
            if (current != null)
            {
                if (changed != null && changed.Id == current?.Id)
                {
                    switch (message)
                    {
                        case "ItemDeleted":
                            await OnItemDeletedExternally();
                            break;
                    }
                }
            }
        }

        private async void OnListMessage(SistemLogListViewModel sender, string message, object args)
        {
            var current = Item;
            if (current != null)
            {
                switch (message)
                {
                    case "ItemsDeleted":
                        if (args is IList<SistemLogModel> deletedModels)
                        {
                            if (deletedModels.Any(r => r.Id == current.Id))
                            {
                                await OnItemDeletedExternally();
                            }
                        }
                        break;
                    case "ItemRangesDeleted":
                        var model = await LogService.SistemLogService.GetSistemLogAsync(current.Id);
                        if (model == null)
                        {
                            await OnItemDeletedExternally();
                        }
                        break;
                }
            }
        }

        private async Task OnItemDeletedExternally()
        {
            await ContextService.RunAsync(() =>
            {
                CancelEdit();
                IsEnabled = false;
                StatusActionMessage($"DİKKAT: Bu {Header} kaydı silinmiş!", StatusMessageType.Warning, autoHide: 5);
            });
        }
    }
}
