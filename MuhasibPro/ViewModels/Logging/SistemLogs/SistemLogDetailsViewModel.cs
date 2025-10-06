﻿using Muhasebe.Business.Models.SistemModel;
using MuhasibPro.Contracts.CommonServices;
using MuhasibPro.ViewModels.Common;

namespace MuhasibPro.ViewModels.Logging.SistemLogs
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

        override public string Title => "Etkinlik Günlükleri";

        public override bool ItemIsNew => false;

        public SistemLogDetailsArgs ViewModelArgs { get; private set; }

        public async Task LoadAsync(SistemLogDetailsArgs args)
        {
            ViewModelArgs = args ?? SistemLogDetailsArgs.CreateDefault();

            try
            {
                var item = await LogService.SistemLogService.GetLogAsync(ViewModelArgs.AppLogID);
                Item = item ?? new SistemLogModel { Id = 0, IsEmpty = true };
            }
            catch (Exception ex)
            {
                LogSistemException("SistemLog", "Load", ex);
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
                StartStatusMessage("Etkinlik günlüğü siliniyor...");
                await Task.Delay(100);
                await LogService.SistemLogService.DeleteLogAsync(model);
                EndStatusMessage("Etkinlik günlüğü silindi");
                return true;
            }
            catch (Exception ex)
            {
                StatusError($"Etkinlik günlüğü silme hatası: {ex.Message}");
                LogSistemException("SistemLog", "Sil", ex);
                return false;
            }
        }

        protected override async Task<bool> ConfirmDeleteAsync()
        {
            return await DialogService.ShowConfirmationAsync("Silmeyi Onayla", "Geçerli günlüğü silmek istediğinizden emin misiniz?", "Tamam", "İptal");
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
                        var model = await LogService.SistemLogService.GetLogAsync(current.Id);
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
                StatusMessage("UYARI: Bu günlük harici olarak silindi");
            });
        }
    }
}
