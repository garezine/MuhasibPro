using FluentValidation;
using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.SistemServices;
using Muhasib.Business.Validations.SistemValidations;
using Muhasib.Domain.Enum;
using Muhasib.Domain.Exceptions;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar;

public class FirmaDetailsArgs
{
    public static FirmaDetailsArgs CreateDefault() => new FirmaDetailsArgs();

    public long FirmaId { get; set; }

    public bool IsNew => FirmaId <= 0;
}

public class FirmaDetailsViewModel : GenericDetailsViewModel<FirmaModel>
{
    public FirmaDetailsViewModel(
        ICommonServices commonServices,
        IFilePickerService filePickerService,
        IFirmaService firmaService) : base(commonServices)
    {
        FilePickerService = filePickerService;
        FirmaService = firmaService;
    }

    public IFilePickerService FilePickerService { get; }

    public IFirmaService FirmaService { get; }

    private string Header => "Firma";

    public override string Title => Item?.IsNew ?? true ? "Yeni Firma" : TitleEdit;

    public string TitleEdit => Item == null ? "Firma" : $"{Item.KisaUnvani}";

    public override bool ItemIsNew => Item?.IsNew ?? true;

    public FirmaDetailsArgs ViewModelArgs { get; private set; }

    public async Task LoadAsync(FirmaDetailsArgs args)
    {
        ViewModelArgs = args ?? FirmaDetailsArgs.CreateDefault();

        if(ViewModelArgs.IsNew)
        {
            var firmaKodu = await FirmaService.GetYeniFirmaKodu();
            Item = new FirmaModel { FirmaKodu = firmaKodu };
            IsEditMode = true;
        } else
        {
            try
            {
                var item = await FirmaService.GetByFirmaIdAsync(ViewModelArgs.FirmaId);

                Item = item.Data ?? new FirmaModel { Id = ViewModelArgs.FirmaId, IsEmpty = true };
            } 
            catch(Exception ex)
            {
                StatusError($"{Header} bilgileri yüklenirken beklenmeyen hata");
                LogSistemException($"{Header}", $"{Header} Detay", ex);
            }
        }
    }

    public void Unload() { ViewModelArgs.FirmaId = Item?.Id ?? 0; }

    public void Subscribe()
    {
        MessageService.Subscribe<FirmaDetailsViewModel, FirmaModel>(this, OnDetailsMessage);
        MessageService.Subscribe<FirmaListViewModel>(this, OnListMessage);
    }

    public void Unsubscribe() { MessageService.Unsubscribe(this); }
    public FirmaDetailsArgs CreateArgs() { return new FirmaDetailsArgs { FirmaId = Item?.Id ?? 0 }; }
    private object _newPictureSource = null;

    public object NewPictureSource { get => _newPictureSource; set => Set(ref _newPictureSource, value); }

    public override void BeginEdit()
    {
        NewPictureSource = null;
        base.BeginEdit();
    }

    public ICommand EditPictureCommand => new RelayCommand(OnEditPicture);

    private async void OnEditPicture()
    {
        NewPictureSource = null;

        await ExecuteActionAsync(
            action: async () =>
            {
                var result = await FilePickerService.OpenImagePickerAsync();
                if(result != null)
                {
                    EditableItem.Logo = result.ImageBytes;
                    EditableItem.LogoSource = result.ImageSource;
                    EditableItem.LogoOnizleme = result.ImageBytes;
                    EditableItem.LogoOnizlemeSource = result.ImageSource;
                    NewPictureSource = result.ImageSource;

                    StatusActionMessage("Logo güncellendi", StatusMessageType.Success, autoHide: 3);
                } else
                {
                    NewPictureSource = null;
                    StatusReady(); // Seçim iptal edildi
                }
            },
            startMessage: "Logo seçiliyor",
            startMessageType: StatusMessageType.Info);
    }

    protected async override Task<bool> SaveItemAsync(FirmaModel model)
    {
        try
        {
            await FirmaService.UpdateFirmaAsync(model);
            LogSistemInformation(
                $"{Header}",
                "Kayıt",
                $"{Header} başarıyla kaydedildi",
                $"{Header} {model.Id} '{model.KisaUnvani}' başarıyla kaydedildi");
            return true;
        } catch(Exception ex)
        {
            StatusError($"{Header} kaydedilirken beklenmeyen hata");
            LogSistemException($"{Header}", "Kayıt", ex);
            return false;
        }
    }

    protected async override Task<bool> DeleteItemAsync(FirmaModel model)
    {
        try
        {
            await FirmaService.DeleteFirmaAsync(model);
            LogSistemWarning($"{Header}", "Sil", $"{Header} silindi", $"'{TitleEdit}' silindi");
            return true;
        }  catch(Exception ex)
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
            $"{Header}'yı silmek istediğinize emin misiniz?",
            "Sil",
            "İptal");
    }
    protected override IEnumerable<AbstractValidator<FirmaModel>> GetValidationConstraints(FirmaModel model)
    { yield return new FirmaValidator(); }

    async void OnDetailsMessage(FirmaDetailsViewModel sender, string message, FirmaModel changed)
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
                                    var item = await FirmaService.GetByFirmaIdAsync(current.Id);
                                    item.Data = item.Data ?? new FirmaModel { Id = current.Id, IsEmpty = true };
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

    private async void OnListMessage(FirmaListViewModel sender, string message, object args)
    {
        var current = Item;
        if(current != null)
        {
            switch(message)
            {
                case "ItemsDeleted":
                    if(args is IList<FirmaModel> deletedModels)
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
                        var model = await FirmaService.GetByFirmaIdAsync(current.Id);
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
