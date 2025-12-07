using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Models.TenantModel;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.SistemServices;
using Muhasib.Domain.Enum;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.SistemViewModel.MaliDonemler
{
    public class MaliDonemDetailsArgs
    {
        public static MaliDonemDetailsArgs CreateDefault() => new MaliDonemDetailsArgs();

        public long FirmaId { get; set; }

        public long MaliDonemId { get; set; }

        public bool IsNew => MaliDonemId <= 0;
    }

    public class MaliDonemDetailsViewModel : GenericDetailsViewModel<MaliDonemModel>
    {
        public MaliDonemDetailsViewModel(
            ICommonServices commonServices,
            IMaliDonemService maliDonemService
,
            ITenantSQLiteWorkflowService workflowService) : base(commonServices)
        {
            MaliDonemService = maliDonemService;
            WorkflowService = workflowService;
        }

        public IMaliDonemService MaliDonemService { get; }
        public ITenantSQLiteWorkflowService WorkflowService { get; }



        private string Header => "Mali Dönem";

        public override string Title => base.Title;

        public string TitleNew => Item?.FirmaModel == null
            ? "Yeni Mali Dönem"
            : $"Yeni Mali Dönem, {Item?.FirmaModel.KisaUnvani}";

        public string TitleEdit => Item == null ? "Mali Dönem" : $"Mali Dönem #{Item?.Id}";

        public override bool ItemIsNew => Item?.IsNew ?? true;

        public bool CanEditFirma => Item?.FirmaId <= 0;
        public long FirmaId { get; set; }
        public ICommand FirmaSelectedCommand => new RelayCommand<FirmaModel>(FirmaSelected);

        private void FirmaSelected(FirmaModel model)
        {
            EditableItem.FirmaId = model.Id;
            EditableItem.FirmaModel = model;
            EditableItem.NotifyChanges();
        }

        public MaliDonemDetailsArgs ViewModelArgs { get; private set; }

        public async Task LoadAsync(MaliDonemDetailsArgs args)
        {
            ViewModelArgs = args ?? MaliDonemDetailsArgs.CreateDefault();
            FirmaId = ViewModelArgs.FirmaId;
            if(ViewModelArgs.IsNew)
            {
                var item = await MaliDonemService.CreateNewMaliDonemAsync(FirmaId);
                Item = item?.Data;                
                IsEditMode = true;
            } else
            {
                try
                {
                    var item = await MaliDonemService.GetByMaliDonemIdAsync(ViewModelArgs.MaliDonemId);
                    Item = item.Data ?? new MaliDonemModel
                    {                       
                        Id = ViewModelArgs.MaliDonemId,
                        IsEmpty = true 
                    };
                } catch(Exception ex)
                {
                    StatusError($"{Header} bilgileri yüklenirken beklenmeyen hata");
                    LogSistemException($"{Header}", $"{Header} Detay", ex);
                }
            }
            NotifyPropertyChanged(nameof(ItemIsNew));
        }

        public void Unload()
        {
            ViewModelArgs.FirmaId = Item?.FirmaId ?? 0;            
            ViewModelArgs.MaliDonemId = Item?.Id ?? 0;            
        }

        public void Subscribe()
        {
            MessageService.Subscribe<MaliDonemDetailsViewModel, MaliDonemModel>(this, OnDetailsMessage);
            MessageService.Subscribe<MaliDonemListViewModel>(this, OnListMessage);
        }

        public void Unsubscribe() { MessageService.Unsubscribe(this); }
        public MaliDonemDetailsArgs CreateArgs()
        { return new MaliDonemDetailsArgs { FirmaId = Item?.FirmaId ?? 0, MaliDonemId = Item?.Id ?? 0, }; }

        protected async override Task<bool> DeleteItemAsync(MaliDonemModel model)
        {
            try
            {
                var request = new TenantDeletingRequest
                {
                    MaliDonemId = model.Id

                };
                bool success = false;
                await ExecuteWithProgressAsync(
                     action: async () =>
                     {
                         var result = await WorkflowService.DeleteTenantCompleteAsync(request);
                         if (result.Success)
                         {
                             success = true;
                             LogSistemInformation(
                                 $"{Header}",
                                 "Sil",
                                 $"{Header} başarıyla silindi",
                                 $"{Header} {model.Id} '{model.MaliYil}' başarıyla silindi");
                             NotificationService.ShowSuccess(Header, "Başarıyla silindi");
                             IsEditMode = false;
                         }
                         else
                         {                             
                             NotificationService.ShowError("Veritabanı hatası:", result.Message);
                             LogSistemError(
                                 $"{Header}",
                                 "Sil",
                                 $"{Header} 'Veritabanı' oluşturulamadı.",
                                 $"{Header} {model.FirmaModel.FirmaKodu} '{model.MaliYil}' veritabanı oluşturulamadı");
                         }
                     },
                     progressMessage: "Veritabanı siliniyor",
                     measureTime: true,
                     successMessage: "Veritabanı silindi",
                     successAutoHideSeconds: -1);
                return success;
            } catch(Exception ex)
            {
                StatusError($"{Header} silinirken beklenmeyen hata");
                LogSistemException($"{Header}", "Sil", ex);
                return false;
            }
        }

        protected async override Task<bool> ConfirmDeleteAsync()
        {
            return await DialogService.ShowAsync(
                "Silme Onayı",
                $"İlgili {Header}'e ait tüm veriler silinecek! {Header}'i silmek istediğinize emin misiniz?",
                "Sil",
                "İptal");
        }

        protected async override Task<bool> SaveItemAsync(MaliDonemModel model)
        {
            try
            {                
                var request = new TenantCreationRequest
                {
                    FirmaId = model.FirmaId,
                    MaliYil = model.MaliYil,
                    AutoCreateDatabase = true,
                    RunMigrations = true,
                    CreateInitialBackup = true,
                };
                bool success = false;
                await ExecuteWithProgressAsync(
                    action: async () =>
                    {
                        var result = await WorkflowService.CreateNewTenantAsync(request);
                        if(result.Success)
                        {
                            success = true;
                            LogSistemInformation(
                                $"{Header}",
                                "Kayıt",
                                $"{Header} başarıyla oluşturuldul",
                                $"{Header} {model.Id} '{model.MaliYil}' başarıyla oluşturuldu");
                        } else
                        {
                            NotifyPropertyChanged(nameof(CanEditFirma));
                            var validationResult = ValidateModel(model);
                            validationResult.IsValid = false;
                            NotificationService.ShowError("Veritabanı hatası:", result.Message);
                            LogSistemError(
                                $"{Header}",
                                "Kayıt",
                                $"{Header} 'Veritabanı' oluşturulamadı.",
                                $"{Header} {model.FirmaModel.FirmaKodu} '{model.MaliYil}' veritabanı oluşturulamadı");
                        }
                    },
                    progressMessage: "Veritabanı oluşturuluyor",
                    measureTime: true,
                    successMessage: "Veritabanı oluşturuldu",
                    successAutoHideSeconds: -1);
                return success;
            } catch(Exception ex)
            {
                StatusError($"{Header} kaydedilirken beklenmeyen hata");
                LogSistemException($"{Header}", "Kayıt", ex);
                return false;
            }
        }

        private async void OnDetailsMessage(MaliDonemDetailsViewModel sender, string message, MaliDonemModel changed)
        {
            var current = Item;
            if(current != null)
            {
                if(changed != null && changed.Id == current?.Id)
                {
                    switch(message)
                    {
                        case "ItemChanged":
                            await ContextService.RunAsync(
                                async () =>
                                {
                                    try
                                    {
                                        var item = await MaliDonemService.GetByMaliDonemIdAsync(current.Id);
                                        item.Data = item.Data ?? new MaliDonemModel { Id = current.Id, IsEmpty = true };
                                        current.Merge(item.Data);
                                        current.NotifyChanges();
                                        NotifyPropertyChanged(nameof(Title));
                                        if(IsEditMode)
                                        {
                                            StatusActionMessage(
                                                $"DİKKAT: Bu {Header} başkası tarafından değiştirildi!",
                                                StatusMessageType.Warning,
                                                autoHide: 5);
                                        }
                                    } catch(Exception ex)
                                    {
                                        StatusError($"{Header} bilgileri güncellenirken hata");
                                        LogSistemException($"{Header}", "Değiştirilmiş", ex);
                                    }
                                });
                            break;
                        case "ItemDeleted":
                            await OnItemDeletedExternally();
                            break;
                    }
                }
            }
        }

        private async Task OnItemDeletedExternally()
        {
            await ContextService.RunAsync(
                () =>
                {
                    CancelEdit();
                    IsEnabled = false;
                    StatusActionMessage($"DİKKAT: Bu {Header} kaydı silinmiş!", StatusMessageType.Warning, autoHide: 5);
                });
        }

        private async void OnListMessage(MaliDonemListViewModel sender, string message, object args)
        {
            var current = Item;
            if(current != null)
            {
                switch(message)
                {
                    case "ItemsDeleted":
                        if(args is IList<MaliDonemModel> deletedModels)
                        {
                            if(deletedModels.Any(r => r.Id == current.Id))
                            {
                                await OnItemDeletedExternally();
                            }
                        }
                        break;
                    case "ItemRangesDeleted":
                        try
                        {
                            var model = await MaliDonemService.GetByMaliDonemIdAsync(current.Id);
                            if(model == null)
                            {
                                await OnItemDeletedExternally();
                            }
                        } catch(Exception ex)
                        {
                            LogSistemException($"{Header}", $"{Header} kaydı silinmiş!", ex);
                        }
                        break;
                }
            }
        }
    }
}
